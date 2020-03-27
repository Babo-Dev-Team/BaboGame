#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <ctype.h>

#define USRN_LENGTH 32
#define GAME_LENGTH 32

int main(int argc, char *argv[])
{
	int sock_conn, sock_listen, request_length;
	struct sockaddr_in serv_addr;
	char request[512];
	char response[512];
	
	// INICIALITZACIONS
	// Obrim el socket
	if ((sock_listen = socket(AF_INET, SOCK_STREAM, 0)) < 0)
		printf("Error creant socket");
	
	// Fem el bind al port
	memset(&serv_addr, 0, sizeof(serv_addr)); // inicialitza a zero serv_addr
	serv_addr.sin_family = AF_INET;
	
	// asocia el socket a cualquiera de las IP de la maquina. 
	// htonl formatea el numero que recibe al formato necesario
	serv_addr.sin_addr.s_addr = htonl(INADDR_ANY);
	// escucharemos en el port 9050
	serv_addr.sin_port = htons(9092);
	if (bind(sock_listen, (struct sockaddr *) &serv_addr, sizeof(serv_addr)) < 0)
		printf ("Error al bind");
	//La cola de requestes pendientes no podr? ser superior a 4
	if (listen(sock_listen, 2) < 0)
		printf("Error en el Listen");
	
	// Atenderemos requestes indefinidamente
	while(1)
	{
		printf ("Escuchando\n");	
		
		//sock_conn es el socket que usaremos para este cliente
		sock_conn = accept(sock_listen, NULL, NULL);
		printf ("He recibido conexi?n\n");
		
		int disconnect = 0;
		while(!disconnect)
		{
			// Ahora recibimos el mensaje, que dejamos en request
			request_length = read(sock_conn, request, sizeof(request));
			printf ("Recibido\n");
			
			// marcamos el final de string
			request[request_length]='\0';
			
			char *p = strtok(request, "/");
			int request_code =  atoi(p); // sacamos el request_code del request
			
			switch (request_code)
			{	
			// request 1 -> Total time played by user query
			// 				client request contains: 	username
			// 				server response contains:	time in HH:MM:SS format	
			case 1:
				{
					char username[USRN_LENGTH];
					strcpy(username, strtok(NULL, "/"));
					
					// realitzar la query
					
					strcpy(response, "test response 1");
					break;
				}
				
			// request 2 -> Winner given a game name query
			// 				client request contains: 	the name of the game
			// 				server response contains:	winner				
			case 2:
				{
					char game_name[GAME_LENGTH];
					char winner[USRN_LENGTH];
					strcpy(game_name, strtok(NULL, "/"));
					
					// realitzar la query
					
					strcpy(response, "test response 2");
					break;
				}
			
			// request 3 -> Characters used in a game by each user query
			// 				client request contains: 	the name of the game
			// 				server response contains:	each user*character pair separated by '/'	
			case 3:
				{
					char game_name[GAME_LENGTH];
					char query_response[200];
					strcpy(game_name, strtok(NULL, "/"));
					
					// realitzar la query
					
					strcpy(response, "test response 3");
					break;
				}

			// request 0 -> Disconnect	
			case 0:
				// Se acabo el servicio para este cliente
				disconnect = 1;
				break;
				
			default:
				printf("Error. request_code no reconocido.\n");
				break;
			}
			
			// Enviamos response siempre que no se haya recibido request de disconnect
			if(request_code)
			{
				printf ("%s\n", response);
				write (sock_conn, response, strlen(response));	
			}
		}
		close(sock_conn);
	}
}

