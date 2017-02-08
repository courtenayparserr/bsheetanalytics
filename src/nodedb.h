#include "was/storage_account.h"
#include "was/queue.h"


void save(bool filtered = false, char *givenfilename = NULL) {
//	static int firstsave = TRUE;
    gzFile f = gzopen(givenfilename ? givenfilename : databasetemp, "wb1h");
    if (!f) panic("PT: could not open database file for writing");
    wint(f, FILE_FORMAT_VERSION);
    wint(f, 'PTFF');
    wint(f, MAXTAGS);
    loop(i, MAXTAGS) {
        gzwrite(f, tags[i].name, sizeof(tags[i].name));
        wint(f, tags[i].color);
    }
    wint(f, minfilter.ival * 60);
    wint(f, foldlevel);
    loop(i, NUM_PREFS) wint(f, prefs[i].ival);
    root->save(f, filtered);
    gzclose(f);
    if (!givenfilename) {
        lastsavetime = GetTickCount();
        // only delete previous backups if we have a database
        if (GetFileAttributesA(databasemain) != INVALID_FILE_ATTRIBUTES) {
/*            if (firstsave) {
                SYSTEMTIME st;
                GetLocalTime(&st);
                char lrun[MAX_PATH];
                sprintf_s(lrun, MAX_PATH, "%sdb_BACKUP_%d_%02d_%02d.PT", databaseroot, st.wYear,
                          st.wMonth, st.wDay);
                DeleteFileA(lrun);
                CopyFileA(databasemain, lrun, FALSE);  // backup last saved of last run once per day
                firstsave = FALSE;
            }*/
            DeleteFileA(databaseback);
            MoveFileA(databasemain, databaseback);  // backup last saved
        }
        MoveFileA(databasetemp, databasemain);
    }
}

void load(node *root, char *fn, bool merge) {
    // FF: struct DATABASE {
    gzFile f = gzopen(fn, "rb");
    if (!f) return;  // inform the user if its not his first run?
    // FF: int version (== 10, as of v1.6, format may be different if other version)
    int version = rint(f);
    if (version > FILE_FORMAT_VERSION) panic("PT: trying to load db from newer version");
    // FF: int magic (== 'PTFF' on x86)
    if (version >= 6 && rint(f) != 'PTFF') panic("PT: not a PT database file");
    if (version >= 4) {
        // don't overwrite global data
        if (merge) {
            int ntags = rint(f);
            uint bytes = ntags * (sizeof(tag().name) + sizeof(int));  // tags
            bytes += version < 6 ? 10 : sizeof(int);                  // minfilter
            bytes += sizeof(int);                                     // foldlevel
            if (version >= 6) {                                       // prefs
                bytes += 5 * sizeof(int);
                if (version >= 10) bytes += sizeof(int);
            }
            loop(i, (int)bytes) gzgetc_s(f);
        } else {
            // FF: int numtags
            int ntags = rint(f);
            if (ntags > MAXTAGS) panic("PT: wrong number of tags in file");
            loop(i, ntags) {
                // FF: struct { char name[32]; int color; } tags[numtags]
                gzread_s(f, tags[i].name, sizeof(tags[i].name));
                int col = rint(f);
                tags[i].color = col;
            }
            // FF: int minfilter
            if (version < 6) {
                loop(i, 10) gzgetc_s(f);
            } else
                minfilter.ival = rint(f) / 60;
            // FF: int foldlevel
            foldlevel = rint(f);
//            ASSERT(NUM_PREFS == 6);
            ASSERT(NUM_PREFS == 7);
			if (version >= 6) {
                // FF: int prefs[6] (see advanced prefs window)
                loop(i, 5) prefs[i].ival = rint(f);
                if (version >= 10) prefs[5].ival = rint(f);
            }
        }
        if (version == 8) loop(i, 24) gzgetc_s(f);
    }
    // FF: NODE root
    root->nname = "";
    root->load(f, version, merge);
    gzclose(f);
    // FF: }
}

void exporthtml(char *fn) {
    FILE *f = fopen(fn, "w");
    if (f) {
        fprintf(f, "<HTML><HEAD><TITLE>balancesheet export</TITLE></HEAD><BODY>");
        root->print(f, true);
        fprintf(f, "</BODY></HTML>");
        fclose(f);
    }
}

void mergedb(char *fn) {
    node *dbr = new node("(root)", NULL);
    load(dbr, fn, true);
    root->merge(*dbr);
    delete dbr;
}

//static char *seperators[] = {" - ", " | ", " : ", " > ", "\\\\", "\\", "//", "/", NULL};
static char *seperators[] = { " ----- ", NULL };

void addtodatabase(char *elements, SYSTEMTIME &st, DWORD idletime, DWORD awaysecs) {
    
	if ((st.wHour < 1 && st.wMinute <= prefs[PREF_AUTOSEND].ival && st.wSecond <= prefs[PREF_SAMPLE].ival) || (lastsenddate.wDay != st.wDay || lastsenddate.wMonth != st.wMonth || lastsenddate.wYear != st.wYear) )	//if a new day
	{
		if (root->onechild)
			root->remove(root->onechild);
		if (root->ht)
			root->ht->clear();

		if (FILE *file = _tfopen(databaseback, _T("rb")))
		{
			fclose(file);
			DeleteFileA(databaseback);
		}
		if (FILE *file = _tfopen(databasemain, _T("rb")))
		{
			fclose(file);
			DeleteFileA(databasemain);
		}
		if (FILE *file = _tfopen(databasetemp, _T("rb")))
		{
			fclose(file);
			DeleteFileA(databasetemp);
		}
	}

	node *n = root;
    for (char *rest = elements, *head; *rest;) {
        head = rest;
        char *sep = NULL;
        int sepl = 0;
        for (char **seps = seperators; *seps; seps++) {
            char *sepn = strstr(rest, *seps);
            if (sepn && (!sep || sepn < sep)) {
                sep = sepn;
                sepl = strlen(*seps);
            }
        }
        if (sep) {
            rest = sep + sepl;
            *sep = 0;
        } else {
            rest = "";
        }
        if (*head) {
            if (head[0] == '[') head++;
            for (;;) {
                size_t l = strlen(head);
                if (l && (head[l - 1] == '*' || head[l - 1] == ']'))
                    head[l - 1] = 0;
                else
                    break;
            }
            if (*head) n = n->add(head);
        }
    }
    n->hit(st, idletime, awaysecs);
}

utility::string_t _MSG = U("");

void send(utility::string_t msg) {

	// Define the connection-string with your values.
	const utility::string_t storage_connection_string(U("DefaultEndpointsProtocol=https;AccountName=beam;AccountKey=YcG8FP9HdaB+r5jDTruTzZy8dXku+wTjV25bkUC4MfLr4hvPcq+C6Uzhh7UOB6C7MemYluQMz28JlzwZIcn6Vw=="));
	// Retrieve storage account from connection string.
	azure::storage::cloud_storage_account storage_account = azure::storage::cloud_storage_account::parse(storage_connection_string);
	// Create a queue client.
	azure::storage::cloud_queue_client queue_client = storage_account.create_cloud_queue_client();
	// Retrieve a reference to a queue.
	azure::storage::cloud_queue queue = queue_client.get_queue_reference(U("beam-queue"));
	// Create the queue if it doesn't already exist.
	queue.create_if_not_exists();

	// Create a message and add it to the queue.
	//azure::storage::cloud_queue_message message(U("[{\"url\":\"https://www.google.com.au\",\"app\":null,\"email\":\"jv@vip.ie\",\"dbId\":\"323e3098-cc87-4b37-8eb5-85a6d6ddba1c\",\"seconds\":147.0490574,\"date\":\"2016-11-17T00:00:00+11:00\"},{\"url\":\"strlen.com/balancesheet/\",\"app\":null,\"email\":\"jv@vip.ie\",\"dbId\":\"323e3098-cc87-4b37-8eb5-85a6d6ddba1c\",\"seconds\":69.1641007,\"date\":\"2016-11-17T00:00:00+11:00\"},{\"url\":\"TimeTracking\",\"app\":null,\"email\":\"jv@vip.ie\",\"dbId\":\"323e3098-cc87-4b37-8eb5-85a6d6ddba1c\",\"seconds\":2.076,\"date\":\"2016-11-17T00:00:00+11:00\"},{\"url\":\"https://github.com/aardappel/balancesheet\",\"app\":null,\"email\":\"jv@vip.ie\",\"dbId\":\"323e3098-cc87-4b37-8eb5-85a6d6ddba1c\",\"seconds\":2.0318743,\"date\":\"2016-11-17T00:00:00+11:00\"},{\"url\":\"zh\",\"app\":null,\"email\":\"jv@vip.ie\",\"dbId\":\"323e3098-cc87-4b37-8eb5-85a6d6ddba1c\",\"seconds\":1.03,\"date\":\"2016-11-17T00:00:00+11:00\"}]"));
	azure::storage::cloud_queue_message message(msg);
	queue.add_message(message);

	lastsendtime = GetTickCount();
}
