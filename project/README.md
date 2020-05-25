# Plan linii Autobusowych
| Nazwisko i imię | Wydział | Kierunek | Semestr | Grupa | Rok akademicki |
| :-------------: | :-----: | :------: | :-----: | :---: | :------------: |
| Mateusz Stabryła        | WIMiIP  | IS       |   4     | 6     | 2019/2020      |

# Projekt bazy danych
Dokładny projekt zawarty jest w pliku /resources/database.vuerd.json, projekt został stworzony przy pomocy rozszerzenia do programu Visual Studio Code o nazwie ERD Editor (https://github.com/vuerd/vuerd-vscode).
Dodatkowo zrobiłem obraz graficzny danego projektu w pliku PlanLiniiAutobusowych.png
Stworzyłem w nim 5 encji: Przystanek, Linia, Przyjazd, Autobus oraz Kierowca.

Encja "Przyjazd" posiada klucze obce do tablic "Linia" oraz "Przystanek".
Encja "Autobus" posiada klucz obcy do tablicy "Linia"
Encja "Kierowca" posiada klucz obcy do tablicy "Autobus"

# Implementacja zapytań SQL
Wszelkie implementacje zapytań SQL zawarłem w osobnych plikach w folderze src/sql dla dwóch rodzajó baz danych MySql oraz MSSQL.
Pliki z frazą "init" inicjalizują bazdę danych z odpowiednimi tablicami oraz relacjami.
Pliki z frazą "insert" zawierają dane początkowe dla mojej aplikacji. Aby dobrze wprowadzić dane do bdazy należy wykonać zapytania w danej kolejności:
*linia*.sql
*przystanek*.sql
*przyjazd*.sql
*aut_kie*.sql
Plik istotne zapytania_z_aplikacji.sql zawiera wszystkie wazniejsze zapytania, któe wykorzystałem w tworzeniu mojej aplikacji.

# Aplikacja
Jest to aplikacja konsolowa podzielona na bibliotekę klas i sam program konsolowy z obsługą komend napisany w języku C# w technologi .NET Core.
Normalnie wykorzystywałem tę aplikację do łączenia się moją prywatną bazą danych. Po zalogowaniu się do bazy można przy pomocy aplikacji wykonać bezpośrednio zapytania SQL lub bardziej złożone operacje wbudowane w ten program jak np. Stworzenie nowej linii i wygenerowanie dla niej rozkładu jazdy.

# Dodatkowe uwagi
Starałem się bardziej skupić na zapytaniach przydatnych w aplikacji. Dlatego jeśli można to prosiłbym o spojrzenie też na plik /src/BDProject.Terminal/Queries.cs w celu oceniania segmentu zapytań SQL oraz kontekstu ich zastosowania.