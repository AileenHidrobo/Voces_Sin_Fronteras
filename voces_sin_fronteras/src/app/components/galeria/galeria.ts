import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface GaleriaItem {
  tipo: 'imagen' | 'video';
  archivo: string;
  titulo: string;
}

@Component({
  selector: 'app-galeria',
  imports: [CommonModule],
  templateUrl: './galeria.html',
  styleUrl: './galeria.css',
})
export class Galeria implements OnInit {
  items: GaleriaItem[] = [];

  modalOpen = false;
  selectedIndex = 0;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.http.get<GaleriaItem[]>('/data/galeria.json')
      .subscribe({
        next: (data) => {
          this.items = data;
        },
        error: () => {
          console.error('No se pudo cargar la galería.');
        }
      });
  }

  openModal(index: number) {
    this.selectedIndex = index;
    this.modalOpen = true;
  }

  closeModal() {
    this.modalOpen = false;
  }

  nextItem() {
    this.selectedIndex =
      (this.selectedIndex + 1) % this.items.length;
  }

  previousItem() {
    this.selectedIndex =
      (this.selectedIndex - 1 + this.items.length) % this.items.length;
  }

  get selectedItem() {
    return this.items[this.selectedIndex];
  }
}