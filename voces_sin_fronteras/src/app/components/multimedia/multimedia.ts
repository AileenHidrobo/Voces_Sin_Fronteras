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

  ngOnInit(): void {
    this.http.get<GalleryItem[]>('/data/galeria.json').subscribe({
      next: (data) => {
        this.galleryItems = Array.isArray(data) ? data : [];
        this.currentGalleryIndex = 0;
      },
      error: (error) => {
        console.error('No se pudo cargar galeria.json', error);
        this.galleryItems = [];
      }
    });
  }

  nextGallery(): void {
    if (this.galleryItems.length === 0) return;

    this.currentGalleryIndex =
      (this.currentGalleryIndex + 1) % this.galleryItems.length;
  }

  previousGallery(): void {
    if (this.galleryItems.length === 0) return;

    this.currentGalleryIndex =
      (this.currentGalleryIndex - 1 + this.galleryItems.length) %
      this.galleryItems.length;
  }

  get currentGalleryItem(): GalleryItem | null {
    if (this.galleryItems.length === 0) {
      return null;
    }

    return this.galleryItems[this.currentGalleryIndex] ?? null;
  }
}