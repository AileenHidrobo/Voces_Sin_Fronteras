import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

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
export class Multimedia {
  galleryItems: GalleryItem[] = [
    {
      tipo: 'imagen',
      archivo: '/galeria/Imagen1.jpeg',
      titulo: 'Proceso de investigación'
    },
    {
      tipo: 'imagen',
      archivo: '/galeria/Imagen2.jpeg',
      titulo: 'Trabajo de campo'
    },
    {
      tipo: 'imagen',
      archivo: '/galeria/Imagen3.jpeg',
      titulo: 'Registro visual'
    },
    {
      tipo: 'video',
      archivo: '/galeria/Video1.mp4',
      titulo: 'Registro audiovisual 1'
    },
    {
      tipo: 'video',
      archivo: '/galeria/Video2.mp4',
      titulo: 'Registro audiovisual 2'
    },
    {
      tipo: 'video',
      archivo: '/galeria/Video3.mp4',
      titulo: 'Registro audiovisual 3'
    }
  ];

  currentGalleryIndex = 0;

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