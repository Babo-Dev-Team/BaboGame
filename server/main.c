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

#define NMBR_THREADS 100
//----------------------------------------------------------
#define CNCTD_LST_LENGTH 20
//Llista d'usuaris connectats
typedef struct{
	int id;
	char username [USRN_LENGTH];
	int socket;
	//int state; //0 lliure, 1 ocupat
}ConnectedUser;

typedef struct ConnectedList{
	ConnectedUser* connected [CNCTD_LST_LENGTH];
	//Connected* connected;
	int number;
	pthread_mutex_t mutex;
}ConnectedList;

typedef struct ThreadArgs{
	ConnectedList* list;
	ConnectedUser* user;
}ThreadArgs;

// creem i afegim connectat a la llista, donats el nom, socket i id
// retorna -1 si la llista està plena
int AddConnectedByAttributes (ConnectedList* list, char name [USRN_LENGTH], int socket, int id)
{
	pthread_mutex_lock(&list->mutex);
	if(list->number < CNCTD_LST_LENGTH)
	{
		int pos = list->number;
		list->connected[pos]->id = id;
		list->connected[pos]->socket = socket;
		strcpy(list->connected[pos]->username, name);
		list->number++;
		pthread_mutex_unlock(&list->mutex);
		return 0;
	}
	
	else
	{
		pthread_mutex_unlock(&list->mutex);
		return -1;
	}
}

// Afegeix user a la llista de connectats (per punter)
// Retorna -1 si la llista està plena
int AddConnected (ConnectedList* list, ConnectedUser* user)
{
	pthread_mutex_lock(&list->mutex);
	if(list->number < CNCTD_LST_LENGTH)
	{
		int pos = list->number;
		list->connected[pos] = user;
		list->number++;
		pthread_mutex_unlock(&list->mutex);
		return 0;
	}
	else
	{
		pthread_mutex_lock(&list->mutex);
		return -1;
	}
	   
}

// retorna la posicio a la llista d'un usuari connectat
// retorna -1 si no s'ha trobat l'usuari
int GetConnectedPos (ConnectedList *list, char name [USRN_LENGTH])
{
	//0 troba la posició, -1 no està a la llista
	int i = 0;
	int user_found = 0;
	pthread_mutex_lock(&list->mutex);
	while ((i < list->number) && !user_found)
	{
		if(!strcmp(list->connected[i]->username, name))
			user_found=1;
		if(!user_found)
			i++;
	}
	pthread_mutex_unlock(&list->mutex);
	if(user_found)
	{
		return i;
	}
	else
	   return -1;
}

// donat un nom, retorna el id de l'usuari connectat
// Retorna -1 si no el troba.
int GetConnectedId (ConnectedList *list, char name [USRN_LENGTH])
{
	//0 troba la posició, -1 no està a la llista
	int i = 0;
	int user_found = 0;
	pthread_mutex_lock(&list->mutex);
	while ((i < list->number) && !user_found)
	{
		if(!strcmp(list->connected[i]->username, name))
			user_found=1;
		if(!user_found)
			i++;
	}
	int ret;
	if(user_found)
	{
		ret = list->connected[i]->id;
	}
	else 
	{
		ret = -1;
	}
	pthread_mutex_unlock(&list->mutex);
	return ret;
}

// Elimina jugador per username
// Retorna -1 si no el troba
int DelConnectedByName(ConnectedList *list, char name[USRN_LENGTH])
{
	// busquem la posicio de l'usuari ja que necessitem mantenir el lock sobre la llista
	int pos = 0;
	int user_found = 0;
	int ret;
	pthread_mutex_lock(&list->mutex);
	while ((pos < list->number) && !user_found)
	{
		if(!strcmp(list->connected[pos]->username, name))
			user_found = 1;
		else pos++;
	}
	if(user_found)
	{
		for (int j = pos; j < list->number - 1; j++)
		{
			list->connected[j] = list->connected[j + 1];
		}
		list->number--;
		ret = 0;
	}
	else 
	   ret = -1;
	pthread_mutex_unlock(&list->mutex);
	return ret;
}

// Elimina jugador per id
// Retorna -1 si no el troba
int DelConnectedById(ConnectedList *list, int id)
{
	// busquem la posicio de l'usuari ja que necessitem mantenir el lock sobre la llista
	int pos = 0;
	int user_found = 0;
	int ret;
	pthread_mutex_lock(&list->mutex);
	while ((pos < list->number) && !user_found)
	{
		if(list->connected[pos]->id == id)
			user_found = 1;
		else pos++;
	}
	if (user_found)
	{
		for (int j = pos; j < list->number - 1; j++)
		{
			list->connected[j] = list->connected[j + 1];
		}
		list->number--;
		ret = 0;
	}
	else 
		ret = -1;
	pthread_mutex_unlock(&list->mutex);
	return ret;
}

//Retorna el socket a partir d'un nom
// Retorna -1 si no troba l'usuari
int  GetConnectedSocket(ConnectedList* list, char name[USRN_LENGTH])
{
	int pos = 0;
	int user_found = 0;
	int ret;
	pthread_mutex_lock(&list->mutex);
	while ((pos < list->number) && !user_found)
	{
		if(!strcmp(list->connected[pos]->username, name))
			user_found = 1;
		else pos++;
	}
	if (user_found)
	{
		ret = list->connected[pos]->socket;
	}
	else
		ret = -1;
	pthread_mutex_unlock(&list->mutex);
	return ret;
}
//-----------------------------------------------------------

//Thread del client
void* attendClient (void* args)
{
	int err = BBDD_connect();
	int sock_conn, request_length;
	ThreadArgs* threadArgs = (ThreadArgs*) args;
	
	// Punters al usuari que gestiona el thread i a la llista
	ConnectedUser* user = threadArgs->user;
	ConnectedList* connectedList = threadArgs->list;
	sock_conn = user->socket;
	
	char username[USRN_LENGTH];
	char password[PASS_LENGTH];
	
	char request[512];
	char response[512];
	
	int disconnect = 0;
	while(!disconnect)
	{
		// Ahora recibimos el mensaje, que dejamos en request
		// no cal bloquejar la llista, doncs user encara no en forma part
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
			strcpy(username, strtok(NULL, "/"));
			strcpy(password, strtok(NULL, "/"));
			printf("User: %s\n", username);
			printf("Password: %s\n", password);
			// realitzar la query
			int id = BBDD_check_login(username, password);
			if(id >= 0)
			{
				// si login OK, omplim els atributs del user
				// i el posem a la llista de connectats.
				// No cal bloquejar la llista, doncs user encara no en forma part
				user->id = id;
				strcpy(user->username, username);
				
				// Aqui hem de bloquejar per afegir user a la llista
				// pero ja ho fa la AddConnected
				int err = AddConnected(connectedList, user);
				if (!err)
				{
					strcpy(response, "OK");					
				}
				else strcpy(response, "FAIL");	
			}
			else 
			{
				strcpy(response, "FAIL");
				disconnect = 1; // close connection to let client try again
			}				
			break;
		}
		
		// request 5 -> Sign Up			
		// 				client request contains: 	the new user and passwd
		// 				server response contains:	OK, FAIL	
		case 5:
		{
			strcpy(username, strtok(NULL, "/"));
			strcpy(password, strtok(NULL, "/"));
			printf("User: %s\n", username);
			printf("Password: %s\n", password);
			//AddConnected(user->list,username,sock_conn);
			// realitzar la query
			int id = BBDD_add_user(username, password);
			if(id >= 0)
			{
				// si sign up OK, afegim els parametres i posem l'usuari a la
				// llista de connectats.
				user->id = id;
				strcpy(user->username, username);
				int err = AddConnected(connectedList, user);
				strcpy(response, "OK");
			}
			else 
			{
				strcpy(response, "USED"); // encara no s'utilitza
			}
			//disconnect = 1; // close connection to let client log in
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
	
	close(user->socket);
	DelConnectedByName(connectedList, user->username);
	
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
	
	//CONNEXIÓ
	int i;
	pthread_t thread[NMBR_THREADS];
	
	//Connectats
	ConnectedList* list = malloc(sizeof(ConnectedList));
	list->number = 0;
	pthread_mutex_init(&list->mutex, NULL);
	
	ThreadArgs threadArgs[NMBR_THREADS];
	for(i = 0; i < NMBR_THREADS; i++)
	{
		threadArgs[i].list = list;
		threadArgs[i].user = malloc(sizeof(ConnectedUser));
	}

	// Atenderemos requestes indefinidamente
	for(i = 0; i < 5; i++)
	{
		printf ("Escuchando\n");	
		
		//sock_conn es el socket que usaremos para este cliente
		sock_conn = accept(sock_listen, NULL, NULL);
		printf ("He recibido conexi?n\n");
		
		threadArgs[i].user->socket = sock_conn;
		pthread_create(&thread[i], NULL, attendClient, &threadArgs[i]);
	}
	
	for(i = 0; i < NMBR_THREADS; i++)
	{
		free(threadArgs[i].user);
	}
	free(list);
}

