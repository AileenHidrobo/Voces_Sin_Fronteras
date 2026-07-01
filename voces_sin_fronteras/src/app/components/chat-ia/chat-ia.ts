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
      text: '¡Hola! Soy la IA del reportaje Voces Sin Fronteras. Solo respondo preguntas sobre esta página.'
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
    }, 50);
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

      this.messages = this.messages.filter(message => message.sender !== 'typing');

      this.messages.push({
        sender: 'bot',
        text: response.answer || 'No pude generar una respuesta.'
      });

      this.updateView();

    } catch (error) {
      this.messages = this.messages.filter(message => message.sender !== 'typing');

      this.messages.push({
        sender: 'bot',
        text: 'No pude conectarme con Gemini. Revisa que el backend VocesApi esté encendido.'
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