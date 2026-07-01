import { Component } from '@angular/core';
import { Navbar } from '../../components/navbar/navbar';
import { Hero } from '../../components/hero/hero';
import { Introduccion } from '../../components/introduccion/introduccion';
import { Contexto } from '../../components/contexto/contexto';
import { Datos } from '../../components/datos/datos';
import { Testimonios } from '../../components/testimonios/testimonios';
import { Multimedia } from '../../components/multimedia/multimedia';
import { Fuentes } from '../../components/fuentes/fuentes';
import { ChatIa } from '../../components/chat-ia/chat-ia';
import { Autores } from '../../components/autores/autores';
import { Footer } from '../../components/footer/footer';

@Component({
  selector: 'app-home',
  imports: [
    Navbar,
    Hero,
    Introduccion,
    Contexto,
    Datos,
    Testimonios,
    Multimedia,
    Fuentes,
    ChatIa,
    Autores,
    Footer
  ],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {}