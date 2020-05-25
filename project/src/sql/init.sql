
        
CREATE TABLE Autobus
(
  id       BIGINT  NOT NULL,
  marka    VARCHAR NOT NULL COMMENT 'marka autobusu',
  model    VARCHAR NOT NULL COMMENT 'konkretny model autobusu',
  id_linia BIGINT  NOT NULL COMMENT 'id linii autobusowej',
  PRIMARY KEY (id)
) COMMENT 'Autobusy na trasie';

CREATE TABLE Kierowca
(
  id_aut      BIGINT  NOT NULL,
  imie        VARCHAR NOT NULL,
  nazwisko    VARCHAR NOT NULL,
  zatrudniony DATE    NOT NULL DEFAULT 1970-01-01 COMMENT 'data zatrudnienia',
  id          bigint  NOT NULL,
  PRIMARY KEY (id)
) COMMENT 'Pracownicy kierujący autobusami';

CREATE TABLE Linia
(
  id          BIGINT NOT NULL,
  nr_linii    INT    NOT NULL DEFAULT 0 COMMENT 'nr linii autobusowej',
  przyst_pocz BIGINT NOT NULL COMMENT 'id przystanku z którego linia zaczyna trasę',
  przyst_kon  BIGINT NOT NULL COMMENT 'id przystanku na którym linia kończy trasę',
  PRIMARY KEY (id)
) COMMENT 'Linia autobusowa';

CREATE TABLE Przyjazd
(
  id        BIGINT   NOT NULL,
  kierunek  BOOL     NULL     DEFAULT 0 COMMENT 'jeśli 0 to przyjazd dotyczy jazdy do przystanku końcowego, a jeśli 1 to dotyczy jazdy do przystanku początkowego.',
  godzina   tim      NOT NULL DEFAULT 00:00:00 COMMENT 'godzina przyjazdu.',
  kolejność SMALLINT NOT NULL DEFAULT 1 COMMENT 'wartość według której jest ukłądana kolejność przyjazdów na dane trasie. Przyjmuje wartości o 1 do n, gdzie n to ilość przystanków.',
  id_linia  BIGINT   NOT NULL COMMENT 'id linii, której dotyczy przyjazd',
  id_przyst BIGINT   NOT NULL COMMENT 'id przystanku, którego dotyczy przyjazd',
  PRIMARY KEY (id)
) COMMENT 'Informacja o przyjeździe na przystanek';

CREATE TABLE Przystanek
(
  id    BIGINT  NOT NULL,
  nazwa VARCHAR NOT NULL COMMENT 'nazwa przystanku',
  NZ    BOOL    NULL     DEFAULT 0 COMMENT 'czy przystanek jest na żądanie',
  PRIMARY KEY (id)
) COMMENT 'Przystanek Autobusowy';

ALTER TABLE Przyjazd
  ADD CONSTRAINT FK_Linia_TO_Przyjazd
    FOREIGN KEY (id_linia)
    REFERENCES Linia (id);

ALTER TABLE Przyjazd
  ADD CONSTRAINT FK_Przystanek_TO_Przyjazd
    FOREIGN KEY (id_przyst)
    REFERENCES Przystanek (id);

ALTER TABLE Linia
  ADD CONSTRAINT FK_Przystanek_TO_Linia
    FOREIGN KEY (przyst_pocz)
    REFERENCES Przystanek (id);

ALTER TABLE Linia
  ADD CONSTRAINT FK_Przystanek_TO_Linia1
    FOREIGN KEY (przyst_kon)
    REFERENCES Przystanek (id);

ALTER TABLE Autobus
  ADD CONSTRAINT FK_Linia_TO_Autobus
    FOREIGN KEY (id_linia)
    REFERENCES Linia (id);

ALTER TABLE Kierowca
  ADD CONSTRAINT FK_Autobus_TO_Kierowca
    FOREIGN KEY (id_aut)
    REFERENCES Autobus (id);

      