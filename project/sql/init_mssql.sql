/*DROP SCHEMA IF EXISTS PlanLiniiAutobusowych;

CREATE SCHEMA PlanLiniiAutobusowych DEFAULT CHARACTER SET utf8;
USE PlanLiniiAutobusowych;*/

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