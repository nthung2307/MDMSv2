/*
Global logger service
 */
export interface ILoggerService {
  warn(...params: any[]): any;
  trace(...params: any[]): any;
  debug(...params: any[]): any;
  info(...params: any[]): any;
  error(...params: any[]): any;
  fatal(...params: any[]): any;
}
