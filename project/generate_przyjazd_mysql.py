import time
queryTemplate = "INSERT INTO `Przyjazd` (`kierunek`, `godzina`, `kolejność`, `id_linia`, `id_przyst`) VALUES ({0},'{1}',{2},{3},{4});\n"
actfile = open("sql/insert_przyjazd.sql","at")
for linia in range(1,4):
    for przystanek in range(24 + linia,40 + linia):
        for cykl in range(5):
            acttime = time.localtime(time.mktime((1970, 1, 1, 9 + cykl, linia + przystanek, 0, 0, 0, 0)))
            newQuery = queryTemplate.format(0,time.strftime("%H:%M:%S",acttime),przystanek - linia,linia,przystanek)
            actfile.write(newQuery)
    for przystanek in range(24 + linia,40 + linia):
        for cykl in range(5):
            acttime = time.localtime(time.mktime((1970, 1, 1, 9 + cykl, linia + przystanek + 30, 0, 0, 0, 0)))
            newQuery = queryTemplate.format(1,time.strftime("%H:%M:%S",acttime),przystanek - linia,linia,przystanek)
            actfile.write(newQuery)