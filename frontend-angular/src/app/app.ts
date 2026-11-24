import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';

interface HelloResponse {
  message: string;
  timeUtc: string;
}

@Component({
  imports: [DatePipe],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  private readonly http = inject(HttpClient);

  protected readonly hello = signal<HelloResponse | null>(null);
  protected readonly error = signal<string | null>(null);

  protected callApi(): void {
    this.error.set(null);
    this.http.get<HelloResponse>(`${environment.apiBaseUrl}/api/hello`).subscribe({
      next: (response) => this.hello.set(response),
      error: (err) => {
        this.hello.set(null);
        this.error.set(err.message);
      },
    });
  }
}
