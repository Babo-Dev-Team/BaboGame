#include <mysql.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>

int main(int argc, char *argv[]) {
	
	MYSQL *connexio;
	int error;
	
	MYSQL_RES *resultat;
	MYSQL_ROW fila;
	char id_partida[20];
	char jugador[20];
	char consulta[500];
	
	connexio=mysql_init(NULL);
	if (connexio==NULL)
	{
		printf("Error en crear la connexió: %u %s\n", mysql_errno(connexio),mysql_error(connexio));
		exit(1);
	}
	
	connexio=mysql_real_connect(connexio,"localhost","root","mysql","BaboGameBBDD",0,NULL,0);
	if (connexio==NULL)
	{
		printf("Error en crear la connexió: %u %s\n", mysql_errno(connexio),mysql_error(connexio));
		exit(1);
	}
	
	printf("Escriu l'dentificador d'una partida:\n");
	scanf("%s",id_partida);
	
	strcat(consulta,"SELECT jugadors.nom FROM jugadors, partides WHERE (partides.id='");
	strcat(consulta,id_partida);
	strcat(consulta,"') AND (partides.idGuanyador=jugadors.id);");
	
	error=mysql_query(connexio,consulta);
	if (error!=0)
	{
		printf("Error en crear la connexió: %u %s\n", mysql_errno(connexio),mysql_error(connexio));
		exit(1);
	}
	
	resultat=mysql_store_result(connexio);
	fila=mysql_fetch_row(resultat);
	
	if (fila==NULL)
	{
		printf("No s'han trobat dades per a la consulta\n");
	}
	else
		{
			while (fila!=NULL)
			{
				printf("%s\n",fila[0]);
				fila=mysql_fetch_row(resultat);
			}
		
	}
	//Com la consulta encomenada s'havia tornat massa senzilla degut a la obligatorietat d'afegir l'identificador del guanyador a la taula de les partides, he decidit fer també aquesta 2a consulta
	printf("La taula de classificació és la següent:\n");
	strcpy(consulta,"SELECT jugadors.nom, COUNT(*) FROM partides, jugadors WHERE jugadors.id=partides.idGuanyador GROUP BY partides.idGuanyador ORDER BY COUNT(*) DESC LIMIT 10;");
	
	error=mysql_query(connexio,consulta);
	if (error!=0)
	{
		printf("Error en crear la connexió: %u %s\n", mysql_errno(connexio),mysql_error(connexio));
		exit(1);
	}
	
	resultat=mysql_store_result(connexio);
	fila=mysql_fetch_row(resultat);
	
	if (fila==NULL)
	{
		printf("No s'han trobat dades per a la consulta\n");
	}
	else
	{
		while (fila!=NULL)
		{
			printf("Jugador: %s  Partides Guanyades: %s\n",fila[0], fila[1]);
			fila=mysql_fetch_row(resultat);
		}
		
	}
}

