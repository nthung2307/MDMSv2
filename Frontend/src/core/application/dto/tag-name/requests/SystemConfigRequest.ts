export class systemConfigRequest {
  configs: { key: string; value: string }[];

  constructor(arg: { configs: { key: string; value: string }[] }) {
    this.configs = arg.configs;
  }
}
