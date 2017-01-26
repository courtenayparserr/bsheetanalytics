CREATE TABLE sessions(id INT PRIMARY KEY, started TEXT NOT NULL);

CREATE TABLE processes(id INT PRIMARY KEY, session_id INT, handle int, name TEXT, FOREIGN KEY(session_id) REFERENCES sessions(id))

CREATE TABLE urls(id INT PRIMARY KEY, session_id INT, url TEXT, FOREIGN KEY(session_id) REFERENCES sessions(id))

CREATE TABLE process_activities(id INT PRIMARY KEY, started TEXT NOT NULL, ended TEXT NULL, process_id int,FOREIGN KEY(process_id) REFERENCES processes(id))

CREATE TABLE url_activities(id INT PRIMARY KEY, started TEXT NOT NULL, ended TEXT NULL, url_id int,FOREIGN KEY(url_id) REFERENCES urls(id))