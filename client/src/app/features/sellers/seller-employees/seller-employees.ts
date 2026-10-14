import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SellerEmployee, SellerService } from '../../../core/auth/seller.service';
import { ConfirmService } from '../../../shared/confirm-dialog/confirm-dialog';

/** Lets a seller invite staff by email, who then act with the same access as the owner. */
@Component({
  selector: 'app-seller-employees',
  standalone: true,
  imports: [FormsModule, MatIconModule],
  templateUrl: './seller-employees.html',
  styleUrl: '../seller-pages.scss',
})
export class SellerEmployees implements OnInit {
  private readonly service = inject(SellerService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly confirmService = inject(ConfirmService);

  readonly employees = signal<SellerEmployee[]>([]);
  readonly isBusy = signal(false);
  email = '';

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.service.getEmployees().subscribe({ next: (rows) => this.employees.set(rows), error: () => this.employees.set([]) });
  }

  invite(): void {
    const email = this.email.trim();
    if (!email) return;
    this.isBusy.set(true);
    this.service.inviteEmployee(email).subscribe({
      next: () => {
        this.isBusy.set(false);
        this.email = '';
        this.snackBar.open(`Invitation emailed to ${email}.`, 'Close');
        this.load();
      },
      error: (err) => {
        this.isBusy.set(false);
        this.snackBar.open(err?.error?.errors?.join(' ') ?? 'Could not send the invitation.', 'Close');
      },
    });
  }

  toggle(e: SellerEmployee): void {
    this.service.setEmployeeActive(e.id, !e.isActive).subscribe({
      next: () => this.load(),
      error: (err) => this.snackBar.open(err?.error?.errors?.join(' ') ?? 'Could not update.', 'Close'),
    });
  }

  remove(e: SellerEmployee): void {
    this.confirmService
      .confirm({
        title: e.isPending ? 'Cancel invitation' : 'Remove employee',
        message: `${e.isPending ? 'Cancel the invitation for' : 'Remove'} ${e.email}?`,
        confirmText: 'Remove',
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.removeEmployee(e.id).subscribe({
          next: () => this.load(),
          error: (err) => this.snackBar.open(err?.error?.errors?.join(' ') ?? 'Could not remove.', 'Close'),
        });
      });
  }
}
