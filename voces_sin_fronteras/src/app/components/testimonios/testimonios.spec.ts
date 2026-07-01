import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Testimonios } from './testimonios';

describe('Testimonios', () => {
  let component: Testimonios;
  let fixture: ComponentFixture<Testimonios>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Testimonios],
    }).compileComponents();

    fixture = TestBed.createComponent(Testimonios);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
