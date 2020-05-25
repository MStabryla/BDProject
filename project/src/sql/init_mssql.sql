CREATE TABLE Linia
(
  id          BIGINT NOT NULL IDENTITY(1,1),
  nr_linii    INT    NOT NULL DEFAULT 0,
  przyst_pocz BIGINT NOT NULL,
  przyst_kon  BIGINT NOT NULL,
  PRIMARY KEY (id)
);

CREATE TABLE Przyjazd
(
  id        BIGINT   NOT NULL IDENTITY(1,1),
  kierunek  TINYINT     NULL     DEFAULT 0 ,
  godzina   TIME      NOT NULL DEFAULT '00:00:00' ,
  kolejność SMALLINT NOT NULL DEFAULT 1 ,
  id_linia  BIGINT   NOT NULL ,
  id_przyst BIGINT   NOT NULL ,
  PRIMARY KEY (id)
) ;

CREATE TABLE Przystanek
(
  id    BIGINT  NOT NULL IDENTITY(1,1),
  nazwa VARCHAR(50) NOT NULL,
  NZ    TINYINT   NULL     DEFAULT 0,
  PRIMARY KEY (id)
);

CREATE TABLE Autobus
(
  id       BIGINT  NOT NULL IDENTITY(1,1),
  marka    VARCHAR(50) NOT NULL,
  model    VARCHAR(50) NOT NULL,
  id_linia BIGINT  NOT NULL,
  CONSTRAINT PK_Autobus PRIMARY KEY (id)
);
CREATE TABLE Kierowca
(
  id            BIGINT  NOT NULL IDENTITY(1,1),
  imie          VARCHAR(50) NOT NULL,
  nazwisko      VARCHAR(50) NOT NULL,
  zatrudniony   DATE    NOT NULL DEFAULT '1970-01-01',
  id_aut        bigint  NOT NULL,
  CONSTRAINT PK_Kierowca PRIMARY KEY (id)
);


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
  ADD CONSTRAINT FK_Autobus_TO_Linia
    FOREIGN KEY (id_linia)
    REFERENCES Linia (id);

ALTER TABLE Kierowca
  ADD CONSTRAINT FK_Autobus_TO_Kierowca
    FOREIGN KEY (id_aut)
    REFERENCES Autobus (id);