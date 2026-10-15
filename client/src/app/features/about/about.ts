import { AfterViewInit, Component, ElementRef, OnDestroy, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { SellerRequestService } from '../../core/auth/seller-request.service';

interface TeamMember {
  name: string;
  title: string;
  description: string;
  isFounder?: boolean;
}

interface StoryStep {
  kicker: string;
  title: string;
  text: string;
  image: string;
  alt: string;
}

interface Shade {
  name: string;
  note: string;
  image: string;
  alt: string;
}

const IMG = 'assets/bangles/';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './about.html',
  styleUrl: './about.scss',
})
export class About implements AfterViewInit, OnDestroy {
  private readonly host = inject(ElementRef) as ElementRef<HTMLElement>;
  private readonly reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  private revealObserver?: IntersectionObserver;
  private scroller?: HTMLElement;
  private frame = 0;

  readonly activeStep = signal(0);
  readonly sending = signal(false);
  readonly submitted = signal(false);
  readonly error = signal('');

  private readonly requests = inject(SellerRequestService);
  readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    phone: ['', [Validators.required, Validators.pattern(/^[0-9+()\-\s]{7,20}$/)]],
    email: ['', [Validators.required, Validators.email]],
    businessName: [''],
    city: [''],
    craftType: [''],
    website: [''],
    message: [''],
  });

  invalid(control: 'name' | 'phone' | 'email'): boolean {
    const c = this.form.controls[control];
    return c.invalid && (c.touched || c.dirty);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set('Please fill in your name, a valid phone number and a valid email.');
      return;
    }
    const v = this.form.getRawValue();
    const blank = (s: string) => s.trim() || null;
    this.error.set('');
    this.sending.set(true);
    this.requests
      .submit({
        name: v.name.trim(),
        phone: v.phone.trim(),
        email: v.email.trim(),
        businessName: blank(v.businessName),
        city: blank(v.city),
        craftType: blank(v.craftType),
        website: blank(v.website),
        message: blank(v.message),
      })
      .subscribe({
        next: () => {
          this.sending.set(false);
          this.submitted.set(true);
        },
        error: (err) => {
          this.sending.set(false);
          this.error.set(err?.status === 429 ? 'Too many requests. Please try again later.' : (err?.error?.errors?.join(' ') ?? 'Could not send your request. Please try again.'));
        },
      });
  }

  readonly lineup = [
    { image: `${IMG}srividika-emerald-bangles.jpeg`, alt: 'Emerald and gold beaded bangle stack' },
    { image: `${IMG}srividika-red-bangles.jpeg`, alt: 'Red silk thread bangles with gold peacock work' },
    { image: `${IMG}srividika-plum-bangle-set.jpeg`, alt: 'Magenta silk thread bangle set on a black tray' },
    { image: `${IMG}srividika-blue-bangles.jpeg`, alt: 'Royal blue silk thread bangles with lotus stones' },
    { image: `${IMG}srividika-ruby-bangles.jpeg`, alt: 'Gold bangles set with ruby stones' },
  ];

  readonly steps: StoryStep[] = [
    {
      kicker: 'Thread',
      title: 'It starts with silk thread.',
      text: 'Every bangle begins as a plain base, wrapped by hand, turn after turn, in rich silk thread until the colour is just right.',
      image: `${IMG}srividika-blue-bangles.jpeg`,
      alt: 'Royal blue silk thread bangles held in a hand',
    },
    {
      kicker: 'Beads & stones',
      title: 'Set one stone at a time.',
      text: 'Beads, kundan and tiny stones are placed and fixed individually. No moulds, no machines. Just patience and a steady hand.',
      image: `${IMG}srividika-emerald-bangles.jpeg`,
      alt: 'Close view of hand-set green stones and gold beadwork',
    },
    {
      kicker: 'Maker',
      title: 'Behind each piece, a maker.',
      text: 'SRIVIDIKA gives independent makers a storefront of their own, so the person who made it is the person who sells it.',
      image: `${IMG}srividika-red-bangles.jpeg`,
      alt: 'Handmade red bangles with peacock motifs on silk fabric',
    },
    {
      kicker: 'You',
      title: 'Made to be worn, gifted, kept.',
      text: 'From festive sets to everyday stacks, find colours and designs you will not see anywhere else, and know exactly who made them.',
      image: `${IMG}srividika-plum-bangle-set.jpeg`,
      alt: 'A full set of magenta and gold bangles styled with a flower',
    },
  ];

  readonly shades: Shade[] = [
    { name: 'Royal Blue', note: 'Lotus stone work', image: `${IMG}srividika-blue-bangles.jpeg`, alt: 'Blue silk thread bangles' },
    { name: 'Emerald', note: 'Green kundan beads', image: `${IMG}srividika-emerald-bangles.jpeg`, alt: 'Emerald bangle stack' },
    { name: 'Ruby', note: 'Antique gold finish', image: `${IMG}srividika-ruby-bangles.jpeg`, alt: 'Ruby gold bangles' },
    { name: 'Crimson', note: 'Peacock motifs', image: `${IMG}srividika-red-bangles.jpeg`, alt: 'Red peacock bangles' },
    { name: 'Magenta', note: 'Festive full set', image: `${IMG}srividika-plum-bangle-set.jpeg`, alt: 'Magenta bangle set' },
    { name: 'Amethyst', note: 'Flower stone work', image: `${IMG}hero-purple-bangles.jpeg`, alt: 'Purple flower bangles' },
    { name: 'Gold', note: 'Coin and bead detail', image: `${IMG}hero-gold-bangles.jpeg`, alt: 'Gold coin bangles' },
  ];

  readonly sellerPoints = [
    { icon: 'bi-shop', title: 'Your own storefront', text: 'List your bangles, set your prices and tell your story in your own words.' },
    { icon: 'bi-camera', title: 'Show the detail', text: 'Photos that let shoppers see every thread, bead and stone up close.' },
    { icon: 'bi-wallet2', title: 'Get paid directly', text: 'Add your UPI and bank details so customers can pay you with ease.' },
    { icon: 'bi-stars', title: 'Custom requests', text: 'Take made-to-order work from shoppers who want something truly theirs.' },
  ];

  readonly team: TeamMember[] = [
    {
      name: 'Kavya',
      title: 'Founder & Creative Lead',
      description: 'Leading the creative vision, product ideas, brand direction, and the overall SRIVIDIKA journey.',
      isFounder: true,
    },
    ...['Deepika', 'Divya', 'Kavana'].map((name) => ({
      name,
      title: 'Co-Owner',
      description: 'Part of the team helping shape and grow the SRIVIDIKA journey.',
    })),
  ];

  ngAfterViewInit(): void {
    this.setupReveals();
    this.setupSteps();
    this.setupParallax();
  }

  ngOnDestroy(): void {
    this.revealObserver?.disconnect();
    this.scroller?.removeEventListener('scroll', this.onScroll);
    cancelAnimationFrame(this.frame);
  }

  scrollTo(id: string, event: Event): void {
    event.preventDefault();
    this.host.nativeElement.querySelector(`#${id}`)?.scrollIntoView({ behavior: this.reducedMotion ? 'auto' : 'smooth' });
  }

  private setupReveals(): void {
    if (this.reducedMotion || typeof IntersectionObserver === 'undefined') return;

    const root = this.host.nativeElement;
    const targets = root.querySelectorAll<HTMLElement>('.reveal');
    root.classList.add('reveal-ready');
    this.revealObserver = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (!entry.isIntersecting) continue;
          entry.target.classList.add('is-visible');
          this.revealObserver?.unobserve(entry.target);
        }
      },
      { threshold: 0.15, rootMargin: '0px 0px -40px 0px' },
    );
    targets.forEach((target) => this.revealObserver?.observe(target));
  }

  private setupSteps(): void {
    this.updateStep();
  }

  private setupParallax(): void {
    this.scroller = this.host.nativeElement.closest<HTMLElement>('.user-content, .mat-sidenav-content') ?? undefined;
    this.scroller?.addEventListener('scroll', this.onScroll, { passive: true });
  }

  private readonly onScroll = (): void => {
    if (this.frame) return;
    this.frame = requestAnimationFrame(() => {
      this.frame = 0;
      const top = this.scroller?.scrollTop ?? 0;
      if (!this.reducedMotion) this.host.nativeElement.style.setProperty('--hero-shift', `${Math.min(top, 900)}`);
      this.updateStep();
    });
  };

  // The step whose centre is closest to the middle of the viewport is the active one.
  private updateStep(): void {
    const mid = window.innerHeight / 2;
    let best = 0;
    let bestDistance = Infinity;
    this.host.nativeElement.querySelectorAll<HTMLElement>('.step').forEach((step, index) => {
      const rect = step.getBoundingClientRect();
      const distance = Math.abs(rect.top + rect.height / 2 - mid);
      if (distance < bestDistance) {
        bestDistance = distance;
        best = index;
      }
    });
    if (best !== this.activeStep()) this.activeStep.set(best);
  }
}
