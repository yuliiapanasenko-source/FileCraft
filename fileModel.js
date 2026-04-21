class File {
  constructor(id, title, content, tags = []) {
    this.id = id;
    this.title = title;
    this.content = content;
    this.tags = tags;
    this.createdAt = new Date();
  }
}

module.exports = File;