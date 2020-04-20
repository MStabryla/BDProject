/*DROP SCHEMA IF EXISTS PlanLiniiAutobusowych;

CREATE SCHEMA PlanLiniiAutobusowych DEFAULT CHARACTER SET utf8;
USE PlanLiniiAutobusowych;*/

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
  godzina   TIME      NOT NULL DEFAULT 00:00:00 COMMENT 'godzina przyjazdu.',
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