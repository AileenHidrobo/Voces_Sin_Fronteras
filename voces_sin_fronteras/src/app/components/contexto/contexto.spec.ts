import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Contexto } from './contexto';

describe('Contexto', () => {
  let component: Contexto;
  let fixture: ComponentFixture<Contexto>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Contexto],
    }).compileComponents();

    fixture = TestBed.createComponent(Contexto);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
