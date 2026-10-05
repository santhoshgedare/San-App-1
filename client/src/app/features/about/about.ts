import { AfterViewInit, Component, ElementRef, OnDestroy, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

interface TeamMember {
  name: string;
  title: string;
  description: string;
  isFounder?: boolean;
}

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './about.html',
  styleUrl: './about.scss',
})
export class About implements AfterViewInit, OnDestroy {
  private readonly host = inject(ElementRef) as ElementRef<HTMLElement>;
  private revealObserver?: IntersectionObserver;
  private readonly reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  readonly team: TeamMember[] = [
    {
      name: 'Kavya',
      title: 'Founder & Creative Lead',
      description: 'Leading the creative vision, product ideas, brand direction, and the overall SRIVIDIKA journey.',
      isFounder: true,
    },
    ...['Srinivas (Sri)', 'Vijayalakshmi', 'Deepika', 'Divya'].map((name) => ({
      name,
      title: 'Co-Owner',
      description: 'Part of the team helping shape and grow the SRIVIDIKA journey.',
    })),
  ];

  ngAfterViewInit(): void {
    this.setupReveals();
  }

  ngOnDestroy(): void {
    this.revealObserver?.disconnect();
  }

  private setupReveals(): void {
    if (this.reducedMotion || typeof IntersectionObserver === 'undefined') return;

    const targets: NodeListOf<HTMLElement> = this.host.nativeElement.querySelectorAll('.reveal');
    if (!targets.length) return;

    this.host.nativeElement.classList.add('reveal-ready');
    this.revealObserver = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (!entry.isIntersecting) continue;
          entry.target.classList.add('is-visible');
          this.revealObserver?.unobserve(entry.target);
        }
      },
      { threshold: 0.12, rootMargin: '0px 0px -32px 0px' },
    );

    targets.forEach((target) => this.revealObserver?.observe(target));
  }
}
