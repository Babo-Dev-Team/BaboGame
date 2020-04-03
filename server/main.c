#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <ctype.h>
#include <pthread.h>

#include "BBDD_Handler.h"

#define USRN_LENGTH 32
#define GAME_ID_LENGTH 8
#define PASS_LENGTH 32

//----------------------------------------------------------
#define CNCTD_LST_LENGTH 20
//Llista d'usuaris connectats
typedef struct{
	char UserName [USRN_LENGTH];
	int socket;
}Connected;

typedef struct{
	Connected connected [CNCTD_LST_LENGTH];
	int number;
}ListConnected;

//Funci� d'afegir connectat
int AddConnected (ListConnected* list, char name [USRN_LENGTH], int socket)
{
	if (list->number < CNCTD_LST_LENGTH)
	{
		strcpy(list->connected[list->number].UserName, name);
		list->connected[list->number].socket = socket;
		list->number++;
		return 0;
	}
	
	return -1;
}

//Retorna la posici�
int Location (ListConnected *list, char name [USRN_LENGTH])
{
	//0 troba la posici�, -1 no est� a la llista
	int i = 0;
	int user_found = 0;
	while ((i < list->number) && !user_found)
	{
		if(!strcmp(list->connected[i].UserName, name))
			user_found=1;
		if(!user_found)
			i++;
	}
	if(user_found)
	{
		return i;
	}
	else
	   return -1;
}

//Elimina jugador
int DelConnected(ListConnected *list, char name[USRN_LENGTH])
{
	//0 l'elimina correctament, 0 no est� a la llista
	int loc = Location(list, name);
	if (loc != -1)
	{
		for(loc; loc < list->number; loc++)
		{
			list->connected[loc] = list->connected[loc+1];
		}
		list->number--;
		return 0;
	}
	else
		return -1;
}
//Retorna el socket a partir d'un nom
int  GetSocket(ListConnected *list, char name[20])
{
	//Reotorna el valor del socket, -1 si no est� a la llista
	int loc = Location(list,name);
	if(loc != -1)
		return list->connected[loc].socket;
	else
		return -1;
}
//-----------------------------------------------------------

//Thread del client
void* attendClient (void* sockets)
{
	int err = BBDD_connect();
	int sock_conn, request_length;
	int* s;
	s = (int *) sockets;
	sock_conn = *s;
	
	char request[512];
	char response[512];
	
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
		printf("Request: %d\n", request_code);
		
		switch (request_code)
		{	
			// request 1 -> Total time played by user query
			// 				client request contains: 	username
			// 				server response contains:	time in HH:MM:SS format	
		case 1:
		{
			char username[USRN_LENGTH];
			strcpy(username, strtok(NULL, "/"));
			printf("User: %s\n", username);
			// realitzar la query
			char* time_played = BBDD_time_played(username);
			strcpy(response, time_played);
			free(time_played);
			break;
		}
		
		// request 2 -> global player ranking
		// 				client request contains: 	nothing
		// 				server response contains:	n_pairs/each user*games won pair separated by '/'			
		case 2:
		{
			// realitzar la query
			char* ranking_str = BBDD_ranking();
			strcpy(response, ranking_str);
			free(ranking_str);
			//strcpy(response, "test response 2");
			break;
		}
		
		// request 3 -> Characters used in a game by each user query
		// 				client request contains: 	the name of the game
		// 				server response contains:	n_pairs/each user*character pair separated by '/'
		case 3:
		{
			char game_id[GAME_ID_LENGTH];
			strcpy(game_id, strtok(NULL, "/"));
			
			// realitzar la query
			char* characters_str = BBDD_find_characters(game_id);
			strcpy(response, characters_str);
			free(characters_str);
			break;
		}
		
		
		// request 4 -> login			
		// 				client request contains: 	the login user and passwd
		// 				server response contains:	OK, FAIL	
		case 4:
		{
			printf("funciona");
			char username[USRN_LENGTH];
			char password[PASS_LENGTH];
			strcpy(username, strtok(NULL, "/"));
			strcpy(password, strtok(NULL, "/"));
			printf("User: %s\n", username);
			printf("Password: %s\n", password);
			// realitzar la query
			int result = BBDD_check_login(username, password);
			if(!result)
			{
				strcpy(response, "OK");
			}
			else 
			{
				strcpy(response, "FAIL");
				disconnect = 1; // close connection to let client try again
			}				
			//strcpy(response, "test response 3");
			break;
		}
		
		// request 5 -> Sign Up			
		// 				client request contains: 	the new user and passwd
		// 				server response contains:	OK, FAIL	
		case 5:
		{
			char username[USRN_LENGTH];
			char password[PASS_LENGTH];
			strcpy(username, strtok(NULL, "/"));
			strcpy(password, strtok(NULL, "/"));
			printf("User: %s\n", username);
			printf("Password: %s\n", password);
			
			// realitzar la query
			int result = BBDD_add_user(username, password);
			if(!result)
			{
				strcpy(response, "OK");
			}
			else 
			{
				strcpy(response, "USED");
			}
			disconnect = 1; // close connection to let client log in
			//strcpy(response, "test response 3");
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
	
	//Acabar el thread
	pthread_exit(0);
}

int main(int argc, char *argv[])
{
	int sock_conn, sock_listen;
	struct sockaddr_in serv_addr;

	
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
	serv_addr.sin_port = htons(9094);
	if (bind(sock_listen, (struct sockaddr *) &serv_addr, sizeof(serv_addr)) < 0)
		printf ("Error al bind");
	//La cola de requestes pendientes no podr? ser superior a 4
	if (listen(sock_listen, 2) < 0)
		printf("Error en el Listen");
	
	//CONNEXI�
	int i;
	int sockets [100];
	pthread_t thread[100];
	
	// Atenderemos requestes indefinidamente
	for(i=0;i<5;i++)
	{
		printf ("Escuchando\n");	
		
		//sock_conn es el socket que usaremos para este cliente
		sock_conn = accept(sock_listen, NULL, NULL);
		printf ("He recibido conexi?n\n");
		
		sockets[i] = sock_conn;
		pthread_create(&thread[i], NULL, attendClient, &sockets[i]);
	}
}

