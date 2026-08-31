import { FormGroup } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

/** Human-readable message from a ProblemDetails response. */
export function problemDetail(
  err: HttpErrorResponse,
  fallback: string,
): string {
  const body = err.error;
  if (body && typeof body === 'object') {
    return body.detail ?? body.title ?? fallback;
  }
  return fallback;
}

/**
 * Extracts FluentValidation field errors (400 ValidationProblemDetails) keyed
 * by control name (camelCased) and stamps them onto matching form controls as
 * a `server` error. Returns true when any control was stamped.
 */
export function applyFieldErrors(
  form: FormGroup,
  err: HttpErrorResponse,
): boolean {
  if (err.status !== 400) return false;
  const errors = err.error?.errors as Record<string, string[]> | undefined;
  if (!errors) return false;

  let applied = false;
  for (const [key, messages] of Object.entries(errors)) {
    const camel = key.charAt(0).toLowerCase() + key.slice(1);
    const control = form.get(camel);
    if (!control) continue;
    control.setErrors({ server: messages?.[0] ?? 'Invalid value.' });
    applied = true;
  }
  return applied;
}

/** Clears `server` errors left by a previous submit. */
export function clearFieldErrors(form: FormGroup): void {
  for (const control of Object.values(form.controls)) {
    const errors = control.errors;
    if (errors && 'server' in errors) {
      const rest: Record<string, unknown> = {};
      for (const [key, value] of Object.entries(errors)) {
        if (key !== 'server') rest[key] = value;
      }
      control.setErrors(Object.keys(rest).length ? rest : null);
    }
  }
}

/** Native date input value (yyyy-MM-dd) or null. */
export function dateOrNull(value: string): string | null {
  const v = value?.trim();
  return v ? v : null;
}

/** First FluentValidation field message from a 400 ValidationProblemDetails, or undefined. */
export function firstFieldError(
  err: unknown,
): string | undefined {
  const body = (err as { error?: { errors?: Record<string, string[]> } | null })
    ?.error;
  const first = body?.errors ? Object.values(body.errors)[0] : undefined;
  return first?.[0];
}