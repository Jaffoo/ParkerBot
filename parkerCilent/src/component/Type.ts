class PocketMessage {
  public content: string | undefined;
  public url: string | undefined;
  public type: number | undefined;//1-文本，2-图片，3-语音，4-视频
  public color?: string

  public add(msg: string, color?: string): PocketMessage {
    this.content = msg;
    this.type = 1;
    this.color = color;
    return this;
  }
  public addImg(content: string, url: string, color?: string): PocketMessage {
    this.content = content;
    this.url = url;
    this.type = 2
    this.color = color;
    return this;
  }
  public addVoice(content: string, url: string, color?: string): PocketMessage {
    this.content = content;
    this.url = url;
    this.type = 3
    this.color = color;
    return this;
  }
  public addVideo(content: string, url: string, color?: string): PocketMessage {
    this.content = content;
    this.url = url;
    this.type = 4;
    this.color = color;
    return this;
  }
}
export default PocketMessage