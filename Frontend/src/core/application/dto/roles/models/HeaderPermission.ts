export class HeaderEntity {
  key: string;
  name: string;

  constructor(arg: { key: string; name: string }) {
    this.key = arg.key;
    this.name = arg.name;
  }
}
