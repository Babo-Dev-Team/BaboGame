drop database if exists T12_BaboGameBBDD;
create database T12_BaboGameBBDD;

use T12_BaboGameBBDD;

create table jugadors(
id INTEGER,
nom VARCHAR(20),
passwd varchar(20),
actiu bool,
primary key (id)
)ENGINE=InnoDB;

create table partides (
id integer,
nom VARCHAR(32),
dataInici datetime,
dataFinal datetime,
duracio integer,
idGuanyador integer,
primary key (id)	
)ENGINE=InnoDB;

create table participants (
idJugador int,
idPartida int,
personatge varchar(16),
puntsJugador int,
foreign key (idJugador) references jugadors(id),
foreign key (idPartida) references partides(id)
)ENGINE=InnoDB;

insert into jugadors values (1, 'Marc', '1234', 1);
insert into jugadors values (2, 'Maria', 'admin', 1);
insert into jugadors values (3, 'Joan', 'Joan', 1);
insert into jugadors values (4, 'Laia', 'Bcn92', 1);

insert into partides values (1, 'partida', '2020-01-03 10:02:32', '2020-01-03 10:32:42', 1860, 4);
insert into partides values (2, 'CORONAbattle', '2020-01-04 12:42:32', '2020-01-04 13:03:34', 3062, 4);
insert into partides values (3, 'GGcombat', '2020-01-05 23:34:46', '2020-01-05 23:55:02', 1216, 4);
insert into partides values (4, 'the game', '2020-01-06 21:34:46', '2020-01-06 22:12:03', 2237, 2);

insert into participants values (1, 1, 'Babo', 1000);
insert into participants values (3, 1, 'Babo', 2000);
insert into participants values (4, 1, 'Limax', 3000);

insert into participants values (2, 2, 'Kaler', 2000);
insert into participants values (4, 2, 'Limax', 4000);

insert into participants values (1, 3, 'Kaler', 2000);
insert into participants values (2, 3, 'Kaler', 2000);
insert into participants values (3, 3, 'Babo', 3500);
insert into participants values (4, 3, 'Limax', 4500);

insert into participants values (1, 4, 'Babo', 500);
insert into participants values (2, 4, 'Limax', 2500);
insert into participants values (3, 4, 'Swalot', 1500);
insert into participants values (4, 4, 'Kaler', 1100);
