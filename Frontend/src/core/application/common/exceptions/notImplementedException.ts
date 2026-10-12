export default class NotImplementedException extends Error {
  constructor(public message: string) {
    super(message);
    this.name = NotImplementedException.name;
    this.stack = (new Error() as any).stack;
  }
}
