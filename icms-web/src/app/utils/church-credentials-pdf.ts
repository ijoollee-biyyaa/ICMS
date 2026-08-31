import { jsPDF } from 'jspdf';

import { CreateChurchResponse } from '../models/church';

const BRAND = [240, 72, 168] as const;
const BRAND_BLUE = [24, 120, 192] as const;
const INK = [31, 41, 55] as const;
const MUTED = [107, 114, 128] as const;
const LIGHT = [243, 244, 246] as const;

/** Issues a one-time printable record of the church admin account credentials. */
export function downloadChurchCredentialsPdf(
  credentials: CreateChurchResponse,
): void {
  const doc = new jsPDF({ unit: 'mm', format: 'a4' });
  const pageWidth = doc.internal.pageSize.getWidth();
  const margin = 18;

  doc.setFillColor(...BRAND);
  doc.rect(0, 0, pageWidth, 8, 'F');
  doc.setFillColor(...BRAND_BLUE);
  doc.rect(0, 8, pageWidth, 2, 'F');

  doc.setTextColor(...INK);
  doc.setFont('helvetica', 'bold');
  doc.setFontSize(16);
  doc.text('EFGBC ICMS', margin, 26);

  doc.setFontSize(11);
  doc.setFont('helvetica', 'normal');
  doc.setTextColor(...MUTED);
  doc.text('Ethiopian Full Gospel Believers Church', margin, 32);
  doc.text('Church Administrator Account Credentials', margin, 38);

  doc.setDrawColor(...BRAND);
  doc.setLineWidth(0.4);
  doc.line(margin, 44, pageWidth - margin, 44);

  const church = credentials.church;
  drawField(doc, margin, 56, 'Church', church.name);
  drawField(doc, margin, 68, 'Church code', church.code);
  drawField(doc, margin, 80, 'Type', church.type);
  drawField(
    doc,
    margin,
    92,
    'Location',
    [church.subcity, church.city].filter(Boolean).join(', ') || '—',
  );

  doc.setDrawColor(229, 231, 235);
  doc.setLineWidth(0.2);
  doc.line(margin, 106, pageWidth - margin, 106);
  doc.setDrawColor(...BRAND_BLUE);
  doc.setLineWidth(1.2);
  doc.line(margin, 106, pageWidth - margin, 106);

  drawField(doc, margin, 118, 'Admin email', credentials.adminEmail);
  drawField(doc, margin, 132, 'Temporary password', credentials.adminTempPassword);

  doc.setFont('helvetica', 'bold');
  doc.setTextColor(...BRAND_BLUE);
  doc.setFontSize(9);
  doc.text('IMPORTANT — one-time credentials', margin, 150);

  doc.setFont('helvetica', 'normal');
  doc.setTextColor(...MUTED);
  doc.setFontSize(9);
  const note = doc.splitTextToSize(
    'This password is shown only once and cannot be recovered. ' +
      'Share it privately with the church administrator so they can sign in and ' +
      'change it immediately. Keep this document in a safe place.',
    pageWidth - margin * 2,
  );
  doc.text(note, margin, 156);

  doc.setFontSize(8);
  doc.text(
    `Issued ${new Date().toLocaleDateString()} by the district office.`,
    margin,
    286,
  );

  doc.setFont('courier', 'normal');
  doc.setFontSize(7);
  doc.text('efgbc-icms', pageWidth - margin, 286, { align: 'right' });

  doc.save(`church-credentials-${church.code.toLowerCase()}.pdf`);
}

function drawField(
  doc: jsPDF,
  x: number,
  y: number,
  label: string,
  value: string,
): void {
  doc.setFont('helvetica', 'bold');
  doc.setTextColor(...MUTED);
  doc.setFontSize(8);
  doc.text(label.toUpperCase(), x, y);

  doc.setFont('helvetica', 'normal');
  doc.setTextColor(...INK);
  doc.setFontSize(12);
  doc.text(value === '' ? '—' : value, x, y + 6);
}