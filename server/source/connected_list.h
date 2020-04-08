#ifndef CONNECTED_LIST_H
#define CONNECTED_LIST_H

#include "globals.h"

#define CNCTD_LST_LENGTH 20

//Llista d'usuaris connectats
typedef struct ConnectedUser{
	int id;
	char username [USRN_LENGTH];
	int socket;
	pthread_mutex_t user_mutex;
	//int state; //0 lliure, 1 ocupat
}ConnectedUser;

typedef struct ConnectedList{
	ConnectedUser* connected [CNCTD_LST_LENGTH];
	//Connected* connected;
	int number;
	pthread_mutex_t mutex;
}ConnectedList;

int AddConnectedByAttributes (ConnectedList* list, char name [USRN_LENGTH], int socket, int id);
int AddConnected (ConnectedList* list, ConnectedUser* user);
int GetConnectedPos (ConnectedList *list, char name [USRN_LENGTH]);
int GetConnectedId (ConnectedList *list, char name [USRN_LENGTH]);
int DelConnectedByName(ConnectedList *list, char name[USRN_LENGTH]);
int DelConnectedById(ConnectedList *list, int id);
int GetConnectedSocket(ConnectedList* list, char name[USRN_LENGTH]);
json_object* connectedListToJson(ConnectedList* list);
	
#endif
