import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Fuentes } from './fuentes';

describe('Fuentes', () => {
  let component: Fuentes;
  let fixture: ComponentFixture<Fuentes>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Fuentes],
    }).compileComponents();

    fixture = TestBed.createComponent(Fuentes);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
