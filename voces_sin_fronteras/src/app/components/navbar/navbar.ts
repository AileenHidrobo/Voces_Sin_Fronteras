import { Component } from '@angular/core';

@Component({
  selector: 'app-navbar',
  imports: [],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {
  menuOpen = false;

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  closeMenu(): void {
    this.menuOpen = false;
  }

  goToInicio(): void {
    this.closeMenu();

    document
      .getElementById('inicio')
      ?.scrollIntoView({
        behavior: 'smooth',
        block: 'start'
      });
  }
}