/** Full names everywhere: First + Father + Grandfather (never only the first name). */
export function fullName(
  firstName: string | null | undefined,
  fatherName: string | null | undefined,
  grandfatherName: string | null | undefined,
): string {
  return [firstName, fatherName, grandfatherName].filter(Boolean).join(' ');
}

/** Full name of a record that carries the three name parts. */
export function recordName(
  record: {
    firstName?: string | null;
    fatherName?: string | null;
    grandfatherName?: string | null;
  } | null | undefined,
): string {
  if (!record) return '';
  return fullName(record.firstName, record.fatherName, record.grandfatherName);
}

/** Two-letter initials for avatars. */
export function initials(
  firstName: string | null | undefined,
  fatherName: string | null | undefined,
  grandfatherName: string | null | undefined,
): string {
  const full = fullName(firstName, fatherName, grandfatherName).trim();
  if (!full) return '?';
  return full
    .split(' ')
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase();
}