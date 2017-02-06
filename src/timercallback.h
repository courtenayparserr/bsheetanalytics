void makejsontosend();

BOOL CALLBACK EnumChildWindowsCallback(HWND hWnd, LPARAM lp) {
    DWORD *procids = (DWORD *)lp;
    DWORD pid = 0;
    GetWindowThreadProcessId(hWnd, &pid);
    if (pid != procids[0]) procids[1] = pid;
    return TRUE;
}

const wchar_t *GetWC(const char *c)
{
	const size_t cSize = strlen(c) + 1;
	wchar_t* wc = new wchar_t[cSize];
	mbstowcs(wc, c, cSize);

	return wc;
}

VOID CALLBACK timerfunc(HWND hwnd, UINT uMsg, UINT_PTR idEvent, DWORD dwTime) {
	if (changesmade)
	{
		if (dwTime - lastsavetime >
			prefs[PREF_AUTOSAVE].ival * 60 * 1000) {  // dwTime wrapping has NO effect on this!
			save();
			changesmade = false;
			threadrestarthook();
		}

		if (dwTime - lastsendtime >
			prefs[PREF_AUTOSEND].ival * 60 * 1000) {  // dwTime wrapping has NO effect on this!

			_MSG = U("");

			root->checkstrfilter(false);
			daydata d;
			tagstat ts;
			daystats.setsize(0);
			loop(i, endtime - starttime + 1) daystats.push(tagstat());
			root->accumulate(d, ts);

			makejsontosend();

			if (_MSG.length() > 0)
			{
				_MSG += utility::string_t(U("]"));

				send(_MSG);

				GetLocalTime(&lastsenddate);
			}
		}
	}

    DWORD idletime = (dwTime - inputhookinactivity()) / 1000;  // same here
    if (idletime > prefs[PREF_IDLE].ival) {
		if (!changesmade) {
			// save one last time while idle, don't keep saving db while
			// idle, and don't immediately save on resume
			lastsavetime = dwTime;
			lastsendtime = dwTime;
		}
        return;
    }
    SYSTEMTIME st;
    GetLocalTime(&st);
    changesmade = true;
    char exename[MAXTMPSTR];
    *exename = 0;
    char title[MAXTMPSTR];
    *title = 0;
    char url[MAXTMPSTR];
	char __url[MAXTMPSTR] = { 0 };
    *url = 0;
    HWND h = GetForegroundWindow();
    if (h) {
        DWORD procids[] = {0, 0};
        GetWindowThreadProcessId(h, procids);
        procids[1] = procids[0];
        // Windows 8/10 hide the real process in a wrapper process called WWAHost.exe or
        // ApplicationFrameHost.exe,
        // so look for child windows with a different id, then use that instead.
        // From:
        // http://stackoverflow.com/questions/32360149/name-of-process-for-active-window-in-windows-8-10
        EnumChildWindows(h, EnumChildWindowsCallback, (LPARAM)procids);
        HANDLE ph = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ /*|PROCESS_SET_QUOTA*/,
                                FALSE, procids[1]);
        if (ph) {
            DWORD count;
            HMODULE hm;
            if (qfpin) { // we're on Vista or above, only this function allows a 32bit process to
                         // query a 64bit one
                wchar_t uexename[MAXTMPSTR];
                DWORD testl = sizeof(uexename);
                qfpin(ph, 0, uexename, &testl);
                uexename[MAXTMPSTR - 1] = 0;
                WideCharToMultiByte(CP_UTF8, 0, uexename, -1, exename, MAXTMPSTR, NULL, NULL);
                exename[MAXTMPSTR - 1] = 0;
            } else { // we're on XP or 2000
                if (EnumProcessModules(ph, &hm, sizeof(HMODULE), &count))
                    GetModuleFileNameExA(ph, hm, exename, MAXTMPSTR);
            }
            exename[MAXTMPSTR - 1] = 0;
            char *trim = strrchr(exename, '\\');
            if (trim) memmove(exename, trim + 1, strlen(trim));
            char *ext = strrchr(exename, '.');
            if (ext) *ext = 0;
            for (char *p = exename; *p; p++) *p = tolower(*p);
            CloseHandle(ph);
        }
        if (strcmp(exename, "firefox") == 0 || strcmp(exename, "iexplore") == 0 ||
            strcmp(exename, "chrome") == 0 || strcmp(exename, "opera") == 0 ||
            strcmp(exename, "netscape") == 0 || strcmp(exename, "netscape6") == 0 ||
            strcmp(exename, "microsoftedge") == 0) {
            // TODO: can we get a UTF-8 URL out of this somehow, if the URL contains percent encoded
            // unicode chars?
            ddereq(exename, "WWW_GetWindowInfo", "0xFFFFFFFF", url, MAXTMPSTR);
            if (!*url) {
                if (!strcmp(exename, "chrome")) {
                    // Chrome doesn't support DDE, get last url change from it:
                    strncpy(url, current_chrome_url, MAXTMPSTR);
                }
                // FIXME: Edge doesn't support DDE either, but currently no known workaround.
            }

            char *http = strstr(url, "://");
            if (!http) {
                http = url;
            } else {
                http += 3;
            }

			strcpy(__url, http);
			if (char* p = strstr(__url, "\""))
				*p = 0;

            if (strncmp(http, "www.", 4) == 0) http += 4;
            size_t len = strcspn(http, "/\":@\0");
            http[len] = 0;
            memmove(url, http, len + 1);
        }
        wchar_t utitle[MAXTMPSTR];
        GetWindowTextW(h, utitle, MAXTMPSTR);
        utitle[MAXTMPSTR - 1] = 0;
        WideCharToMultiByte(CP_UTF8, 0, utitle, -1, title, MAXTMPSTR, NULL, NULL);
        title[MAXTMPSTR - 1] = 0;
    }
    std::string s = exename;

	if (__url[0] && s[0]) s += "---";
	s += __url;
/*
	if (__url[0] == 0 && s[0])
	{
		if (title[0] && s[0]) s += "---";
		s += title;
	}
*/
	if (s[0])
	{
		addtodatabase((char *)s.c_str(), st, idletime, 0);
	}

};

void makejsontosend() {

	bool filtered = true;

	if (root->ht) {
		int num = root->numnotfiltered(filtered);
		if (num) {
			Vector<node *> v;
			root->ht->getelements(v);
			v.sort((void *)root->nodesorter);

			loopv(i, v) {
				if (v[i]->hidden) continue;

				if (!filtered || v[i]->accum.seconds) {

					wchar_t szElapsedTime[MAXTMPSTR] = { 0 }, uurl[MAXTMPSTR] = { 0 }, uexename[MAXTMPSTR] = { 0 };
					wchar_t szDate[MAXTMPSTR] = { 0 };

					String s;

					//Elapsed time info
					//v[i]->accum.format(s, 0);
					//mbstowcs(szElapsedTime, s.c_str(), MAXTMPSTR);
					/* He required the format in whole seconds*/
					swprintf(szElapsedTime, U("\"%d\""), v[i]->accum.seconds);

					s.Clear();
					//date&activity info
					if (v[i]->accum.nminute && v[i]->accum.nday) {
						SYSTEMTIME st;
						v[i]->accum.createsystime(st);
						s.FormatCat("%d-%d-%dT%d:%02d", st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute);
					}
					mbstowcs(szDate, s.c_str(), MAXTMPSTR);


					//app info
					char _exename[MAXTMPSTR] = { 0 };
					char *p = _exename;
					strcpy(_exename, v[i]->nname);
					if (p = strstr(_exename, "---")) *p = 0;
					mbstowcs(uexename, _exename, MAXTMPSTR);

					//url info
					char _url[MAXTMPSTR] = { 0 };
					p = _url;
					strcpy(_url, v[i]->nname);
					if (p = strstr(_url, "---"))
					{
						p += 3;
						if (*p)	mbstowcs(uurl, p, MAXTMPSTR);
					}

					
					if (_MSG.length() == 0)
					{
						if (wcscmp(uexename, U("null")) == 0)
							_MSG = utility::string_t(U("[{ \"url\":\"")) + utility::string_t(uurl) + utility::string_t(U("\",\"app\" : ")) + utility::string_t(U("null")) + utility::string_t(U(",\"computername\" : \"")) + utility::string_t(GetWC(computerNode.c_str())) + utility::string_t(U("\",\"domain\" : \"")) + utility::string_t(GetWC(computerDomain.c_str())) + utility::string_t(U("\",\"seconds\" : ")) + utility::string_t(szElapsedTime) + utility::string_t(U(",\"date\" : \"")) + utility::string_t(szDate) + utility::string_t(U("\" }"));
						else
							_MSG = utility::string_t(U("[{ \"url\":\"")) + utility::string_t(uurl) + utility::string_t(U("\",\"app\" : \"")) + utility::string_t(uexename) + utility::string_t(U("\",\"computername\" : \"")) + utility::string_t(GetWC(computerNode.c_str())) + utility::string_t(U("\",\"domain\" : \"")) + utility::string_t(GetWC(computerDomain.c_str())) + utility::string_t(U("\",\"seconds\" : ")) + utility::string_t(szElapsedTime) + utility::string_t(U(",\"date\" : \"")) + utility::string_t(szDate) + utility::string_t(U("\" }"));

					}
					else
					{
						if (wcscmp(uexename, U("null")) == 0)
							_MSG += utility::string_t(U(",{ \"url\":\"")) + utility::string_t(uurl) + utility::string_t(U("\",\"app\" : ")) + utility::string_t(U("null")) + utility::string_t(U(",\"computername\" : \"")) + utility::string_t(GetWC(computerNode.c_str())) + utility::string_t(U("\",\"domain\" : \"")) + utility::string_t(GetWC(computerDomain.c_str())) + utility::string_t(U("\",\"seconds\" : ")) + utility::string_t(szElapsedTime) + utility::string_t(U(",\"date\" : \"")) + utility::string_t(szDate) + utility::string_t(U("\" }"));
						else
							_MSG += utility::string_t(U(",{ \"url\":\"")) + utility::string_t(uurl) + utility::string_t(U("\",\"app\" : \"")) + utility::string_t(uexename) + utility::string_t(U("\",\"computername\" : \"")) + utility::string_t(GetWC(computerNode.c_str())) + utility::string_t(U("\",\"domain\" : \"")) + utility::string_t(GetWC(computerDomain.c_str())) + utility::string_t(U("\",\"seconds\" : ")) + utility::string_t(szElapsedTime) + utility::string_t(U(",\"date\" : \"")) + utility::string_t(szDate) + utility::string_t(U("\" }"));
					}
				}//end of if
			}//end of loopv
			v.setsize_nd(0);
		}//end of if (num)
	}

	return;
}