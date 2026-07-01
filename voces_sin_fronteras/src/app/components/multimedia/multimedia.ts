import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface GalleryItem {
  tipo: 'imagen' | 'video';
  archivo: string;
  titulo: string;
}

@Component({
  selector: 'app-multimedia',
  imports: [CommonModule],
  templateUrl: './multimedia.html',
  styleUrl: './multimedia.css',
})
export class Multimedia implements OnInit {
  galleryItems: GalleryItem[] = [];
  currentGalleryIndex = 0;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.http.get<GalleryItem[]>('/data/galeria.json').subscribe({
      next: (data) => {
        this.galleryItems = data;
      },
      error: () => {
        console.error('No se pudo cargar galeria.json');
      }
    });
  }

  nextGallery() {
    this.currentGalleryIndex =
      (this.currentGalleryIndex + 1) % this.galleryItems.length;
  }

  previousGallery() {
    this.currentGalleryIndex =
      (this.currentGalleryIndex - 1 + this.galleryItems.length) % this.galleryItems.length;
  }

  get currentGalleryItem() {
    return this.galleryItems[this.currentGalleryIndex];
  }
}