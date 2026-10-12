/**
 * Provide datetime service
 */
export interface IDateTimeService {
  // Get current datetime and return datetime string
  currentDateTime(format: string): string;

  formatDateTime(date: string, format: string): string;
}
