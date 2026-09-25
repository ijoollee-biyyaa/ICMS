/**
 * Parses a "yyyy-MM-dd" string into a Date using LOCAL calendar parts.
 * `new Date("yyyy-MM-dd")` is parsed as UTC midnight, which shifts the day by
 * one in timezones east or west of UTC. Building the Date from its parts keeps
 * the calendar date stable no matter the machine timezone.
 */
export function toLocalDate(value: string): Date | null {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) return null;
  const [, y, m, d] = match;
  return new Date(Number(y), Number(m) - 1, Number(d));
}

/** Formats a "yyyy-MM-dd" calendar date without timezone day-shift. */
export function formatDateValue(
  value: string,
  opts: Intl.DateTimeFormatOptions = {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  },
): string {
  const date = toLocalDate(value);
  if (!date) return value;
  return date.toLocaleDateString(undefined, opts);
}