import { Component, ElementRef, ViewChild, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

interface Message {
  text: string;
  sender: 'bot' | 'user' | 'typing';
}

@Component({
  selector: 'app-chat-ia',
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-ia.html',
  styleUrl: './chat-ia.css',
})
export class ChatIa {

  @ViewChild('messagesContainer') messagesContainer!: ElementRef;

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  isOpen = false;
  userQuestion = '';
  isLoading = false;
  showQuickQuestions = true;

  apiUrl = 'https://voces-api.onrender.com/api/chat';

  messages: Message[] = [
    {
      sender: 'bot',
      text: '¡Hola! Soy la IA del reportaje Voces Sin Fronteras. Solo respondo preguntas relacionadas con este reportaje sobre migración juvenil ecuatoriana.'
    }
  ];

  toggleChat() {
    this.isOpen = !this.isOpen;
    this.updateView();
  }

  updateView() {
    this.cdr.detectChanges();
    this.scrollToBottom();
  }

  scrollToBottom() {
    setTimeout(() => {
      if (this.messagesContainer) {
        this.messagesContainer.nativeElement.scrollTop =
          this.messagesContainer.nativeElement.scrollHeight;
      }
    }, 80);
  }

  cleanAnswer(text: string): string {
    return text
      .replace(/\*\*/g, '')
      .replace(/\*/g, '')
      .trim();
  }

  async sendMessage() {

    const question = this.userQuestion.trim();

    if (!question || this.isLoading) return;

    this.messages.push({
      sender: 'user',
      text: question
    });

    this.userQuestion = '';
    this.showQuickQuestions = false;
    this.isLoading = true;

    this.updateView();

    this.messages.push({
      sender: 'typing',
      text: '✍️ Escribiendo respuesta...'
    });

    this.updateView();

    try {

      const response: any = await firstValueFrom(
        this.http.post(this.apiUrl, { question })
      );

      this.messages = this.messages.filter(m => m.sender !== 'typing');

      const answer = this.cleanAnswer(
        response.answer || 'No fue posible generar una respuesta.'
      );

      this.messages.push({
        sender: 'bot',
        text: answer
      });

      this.updateView();

    } catch (error: any) {

      this.messages = this.messages.filter(m => m.sender !== 'typing');

      let mensaje =
        'Ocurrió un inconveniente al consultar la inteligencia artificial. Inténtalo nuevamente más tarde.';

      if (error?.status === 429) {

        mensaje =
          'La inteligencia artificial ha alcanzado el límite diario de consultas disponible. Por favor, vuelve a intentarlo más tarde.';

      } else if (error?.status === 400) {

        mensaje =
          'No fue posible procesar la consulta. Intenta formular la pregunta de otra manera.';

      } else if (error?.status === 500) {

        mensaje =
          'La inteligencia artificial no se encuentra disponible en este momento. Inténtalo nuevamente en unos minutos.';

      } else if (error?.status === 0) {

        mensaje =
          'No fue posible establecer conexión con el servidor. Verifica tu conexión a Internet e inténtalo nuevamente.';
      }

      this.messages.push({
        sender: 'bot',
        text: mensaje
      });

      this.updateView();

    } finally {

      this.isLoading = false;
      this.updateView();

    }

  }

  askQuickQuestion(question: string) {

    if (this.isLoading) return;

    this.userQuestion = question;
    this.sendMessage();

  }

}