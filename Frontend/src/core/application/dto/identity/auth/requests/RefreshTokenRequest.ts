export class RefreshTokenRequest {
  refreshToken: string;
  constructor(arg: { refreshToken: string }) {
    this.refreshToken = arg.refreshToken;
  }
}
