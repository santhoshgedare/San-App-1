import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="forbidden">
      <h1>403</h1>
      <p>You don't have permission to view this page.</p>
      <a routerLink="/dashboard">Back to Dashboard</a>
    </div>
  `,
  styles: [
    `
      .forbidden {
        text-align: center;
        padding: 4rem 1rem;
      }
      h1 {
        font-size: 3rem;
        margin: 0;
        color: #dc2626;
      }
      a {
        color: #2563eb;
        font-weight: 600;
      }
    `,
  ],
})
export class Forbidden {}
