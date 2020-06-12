//------------------------------------------------------------------------------
// Funcions per crear, eliminar i gestionar la llista de connectats
// i els seus usuaris, de tipus ConnectedUser.
//------------------------------------------------------------------------------

#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <stdio.h>
#include <ctype.h>
#include <pthread.h>
#include "json.h"

#include "connected_list.h"

//------------------------------------------------------------------------------
//PRIVATE METHODS
//------------------------------------------------------------------------------

// metode per eliminar usuari (privat)
void __deleteConnectedUser(ConnectedUser* user)
{
	free(user);
	
}
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
//PUBLIC METHODS
//------------------------------------------------------------------------------

// creem una llista de connectats. El mutex de la llista 
// el creem i li passem des del programa principal
ConnectedList* CreateConnectedList(pthread_mutex_t* mutex)
{
	ConnectedList* list = malloc(sizeof(ConnectedList));
	list->mutex = mutex;
	list->number = 0;
	return list;
}

// creem un usuari connectat. 
ConnectedUser* CreateConnectedUser()
{
	ConnectedUser* user = malloc(sizeof(ConnectedUser));
	return user;
}


// metode per eliminar llista (públic)
void DeleteConnectedList(ConnectedList* list)
{
	for( int i = 0; i < list->number; i++)
	{
		__deleteConnectedUser(list->connected[i]);
	}
	free(list);
}

// creem i afegim connectat a la llista, donats el nom, socket i id
// Assignem el mutex de la llista a l'usuari (compartim mecanisme d'accés exclusiu)
// retorna -1 si la llista està plena
int AddConnectedByAttributes (ConnectedList* list, char name [USRN_LENGTH], int socket, int id)
{
	pthread_mutex_lock(list->mutex);
	if(list->number < CNCTD_LST_LENGTH)
	{
		int pos = list->number;
		ConnectedUser* user = CreateConnectedUser();
		user->user_mutex = list->mutex;					// li assignem el mutex de la llista
		user->id = id;
		user->socket = socket;
		strcpy(user->username, name);
		list->connected[pos] = user;
		list->number++;
		pthread_mutex_unlock(list->mutex);
		return 0;
	}
	
	else
	{
		pthread_mutex_unlock(list->mutex);
		return -1;
	}
}

// Afegeix user a la llista de connectats (per punter)
// Assignem el mutex de la llista a l'usuari (compartim mecanisme d'accés exclusiu)
// Retorna -1 si la llista està plena
int AddConnected (ConnectedList* list, ConnectedUser* user)
{
	pthread_mutex_lock(list->mutex);
	if(list->number < CNCTD_LST_LENGTH)
	{
		int pos = list->number;
		user->user_mutex = list->mutex;		// li assignem el mutex de la llista
		list->connected[pos] = user;
		list->number++;
		pthread_mutex_unlock(list->mutex);
		return 0;
	}
	else
	{
		pthread_mutex_unlock(list->mutex);
		return -1;
	}
	
}

// retorna la posicio a la llista d'un usuari connectat
// retorna -1 si no s'ha trobat l'usuari
int GetConnectedPos (ConnectedList* list, char name [USRN_LENGTH])
{
	//0 troba la posició, -1 no està a la llista
	int i = 0;
	int user_found = 0;
	pthread_mutex_lock(list->mutex);
	while ((i < list->number) && !user_found)
	{
		if(!strcmp(list->connected[i]->username, name))
			user_found=1;
		if(!user_found)
			i++;
	}
	pthread_mutex_unlock(list->mutex);
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
	pthread_mutex_lock(list->mutex);
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
	pthread_mutex_unlock(list->mutex);
	return ret;
}

// Elimina jugador per username (mètode públic)
// Retorna -1 si no el troba
int DelConnectedByName (ConnectedList *list, char name[USRN_LENGTH])
{
	// busquem la posicio de l'usuari ja que necessitem mantenir el lock sobre la llista
	int pos = 0;
	int user_found = 0;
	int ret;
	pthread_mutex_lock(list->mutex);
	while ((pos < list->number) && !user_found)
	{
		if(!strcmp(list->connected[pos]->username, name))
			user_found = 1;
		else pos++;
	}
	if(user_found)
	{
		__deleteConnectedUser(list->connected[pos]); // eliminem l'objecte en memoria
		for (int j = pos; j < list->number - 1; j++)
		{
			list->connected[j] = list->connected[j + 1]; // desplaçem els punters
		}
		list->number--;
		ret = 0;
	}
	else 
	   ret = -1;
	pthread_mutex_unlock(list->mutex);
	return ret;
}

// Elimina jugador per id (mètode públic)
// Retorna -1 si no el troba
int DelConnectedById(ConnectedList *list, int id)
{
	// busquem la posicio de l'usuari ja que necessitem mantenir el lock sobre la llista
	int pos = 0;
	int user_found = 0;
	int ret;
	pthread_mutex_lock(list->mutex);
	while ((pos < list->number) && !user_found)
	{
		if(list->connected[pos]->id == id)
			user_found = 1;
		else pos++;
	}
	if (user_found)
	{
		__deleteConnectedUser(list->connected[pos]); // eliminem l'objecte en memoria
		for (int j = pos; j < list->number - 1; j++)
		{
			list->connected[j] = list->connected[j + 1];
		}
		list->number--;
		ret = 0;
	}
	else 
		ret = -1;
	pthread_mutex_unlock(list->mutex);
	return ret;
}

// Elimina jugador de la llista (mètode públic)
// Retorna -1 si no el troba
int DelConnectedFromList(ConnectedList* list, ConnectedUser* user)
{
	// busquem la posicio de l'usuari ja que necessitem mantenir el lock sobre la llista
	int pos = 0;
	int user_found = 0;
	int ret;
	pthread_mutex_lock(list->mutex);
	while ((pos < list->number) && !user_found)
	{
		if(list->connected[pos] == user)
			user_found = 1;
		else pos++;
	}
	if (user_found)
	{
		__deleteConnectedUser(list->connected[pos]); // eliminem l'objecte en memoria
		for (int j = pos; j < list->number - 1; j++)
		{
			list->connected[j] = list->connected[j + 1];
		}
		list->number--;
		ret = 0;
	}
	else 
		ret = -1;
	pthread_mutex_unlock(list->mutex);
	return ret;	
}

//Retorna el socket a partir d'un nom
// Retorna -1 si no troba l'usuari
int GetConnectedSocket(ConnectedList* list, char name[USRN_LENGTH])
{
	int pos = 0;
	int user_found = 0;
	int ret;
	pthread_mutex_lock(list->mutex);
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
	pthread_mutex_unlock(list->mutex);
	return ret;
}

// Creem un objecte JSON que representa l'estat de la llista de connectats. 
// Ho farem servir per enviar de forma global aquesta informació.
json_object* connectedListToJson(ConnectedList* list)
{
	//json_object* list_obj = json_object_new_object();
	json_object* list_array = json_object_new_array();
	
	pthread_mutex_lock(list->mutex);
	int n_users = list->number;
	
	//json_object* n_users_obj = json_object_new_int(n_users);
	for(int i = 0; i < n_users; i++)
	{
		json_object* user = json_object_new_object();
		json_object* id = json_object_new_int(list->connected[i]->id);
		json_object* name = json_object_new_string(list->connected[i]->username);
		json_object_object_add(user, "Name", name);
		json_object_object_add(user, "Id", id);
		
		json_object_array_add(list_array, user); 
	}	
	pthread_mutex_unlock(list->mutex);
	//json_object_object_add(list_obj, "n_connected", n_users_obj);
	//json_object_object_add(list_obj, "connectedList", list_array);
	
	return list_array;
}
//-----------------------------------------------------------
