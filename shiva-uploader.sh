echo "Escriu el teu nom d'usuari (nom.cognom):"
read usuari
scp -r server ${usuari}@shiva.upc.es:/home/albert.compte/server
ssh ${usuari}@shiva.upc.es 'cd server; gcc -Isource/ -Ilibs/json-c/ -Ilibs/include/json-c/ -o BaboGame main.c source/connected_list.c source/BBDD_Handler.c source/game_table.c -lpthread `mysql_config --cflags --libs` libs/lib/libjson-c.a'
