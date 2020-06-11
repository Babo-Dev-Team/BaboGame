echo "Escriu el teu nom d'usuari (nom.cognom):"
read usuari
scp -r server ${usuari}@shiva.upc.es:/home/${usuari}
ssh ${usuari}@shiva.upc.es 'cd server; if [ ! -e shiva_release ]; then mkdir shiva_release; fi; gcc -Isource/ -Ilibs/json-c/ -Ilibs/include/json-c/ -o shiva_release/BaboGame main.c source/connected_list.c source/BBDD_Handler.c source/game_table.c source/game_state.c -lpthread `mysql_config --cflags --libs` libs/lib/libjson-c.a'
