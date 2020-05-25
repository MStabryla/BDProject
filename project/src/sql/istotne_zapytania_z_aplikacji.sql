/*Zwraca wszystkie linie autobusowe */
SELECT nr_linii FROM Linia;
/*Zwraca wszystkie przystanki autobusowe */
SELECT nazwa,NZ FROM Przystanek;
/*Zwraca wszystkie przystanki, na których dana linia się zatrzymuje*/
SELECT nazwa FROM Przystanek INNER JOIN Przyjazd ON [Przystanek].[id] = [Przyjazd].[id_przyst] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[id] = {id_linia} AND [Przyjazd].[kierunek] = 0 GROUP BY nazwa,[Przyjazd].[kolejność] ORDER BY [Przyjazd].[kolejność]
/*Zwraca wszystkie linie, które zatrzymują się na danym przystanku*/
SELECT [Linia].[nr_linii] FROM Linia INNER JOIN Przyjazd ON [Linia].[id] = [Przyjazd].[id_linia] RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] WHERE [Przystanek].[id] = {id_przyst} AND [Przyjazd].[kierunek] = 0 GROUP BY [Linia].[nr_linii],[Przyjazd].[kolejność] ORDER BY [Przyjazd].[kolejność]
/*Zwraca rozklad jazdy dla danego przystanku*/
SELECT [Linia].[nr_linii],[Przyjazd].[godzina] FROM Przyjazd RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Przystanek].[id] = {id_przyst} AND [Przyjazd].[kierunek] = {kierunek} ORDER BY [Przyjazd].[godzina]
/*Zwraca rozklad jazdy dla danej linii*/
SELECT [Przystanek].[nazwa],[Przyjazd].[godzina] FROM Przyjazd RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[id] = {id_linia} AND [Przyjazd].[kierunek] = {kierunek} ORDER BY [Przyjazd].[godzina] 
/*Zwraca wszystkie przystanki poza ostatnim */
DECLARE @PrzystanekCount INT;SELECT @PrzystanekCount = Count(id) FROM Przystanek;SELECT TOP (@PrzystanekCount - 1) [id], [nazwa] FROM Przystanek;
/*Usunięcie wszystkich przyjazdów dla danej linii*/
DELETE FROM Przyjazd WHERE [id_linia] = {id_linia}
/*Usunięcie linii*/
DELETE FROM Linia WHERE [id] = {id_linia}
/*Usunięcie wszystkich przyjazdów dla danego przystanku*/
DELETE FROM Przyjazd WHERE [id_przyst] = {id_przyst}
/*Usunięcie przystanku*/
DELETE FROM Przystanek WHERE [id] = {id_przyst}
/*Lista wszystkich przystanów początkowych i końcowych*/
SELECT [Przystanek].[id], nazwa FROM Przystanek INNER JOIN Przyjazd ON [Przystanek].[id] = [Przyjazd].[id_przyst] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[przyst_pocz] = [Przystanek].[id] OR [Linia].[przyst_kon] = [Przystanek].[id] GROUP BY [Przystanek].[id],nazwa
/*Wybranie godziny początkowej z oryginalnej trasy*/
SELECT TOP(1) [godzina] FROM Przyjazd RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[id] = {id_linia}
/*Zmiana przystanku początkowego i końcowego trasy*/
UPDATE Linia SET [przyst_pocz] = {id_przyst_p}, [przyst_kon] = {id_przyst_p} WHERE [id] = {id_linia}