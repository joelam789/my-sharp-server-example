
export class GameMedia {

    game: any = null;
    gameWrapper: any = null;
    stream: any = null;

    loadingContainer: any = null;
    appContainer: any = null;
    gameContainer: any = null;
    videoContainer: any = null;

    onEnterFrame: () => void = null;

    loadContainers() {
        this.loadingContainer = document.getElementById('loading');
        this.appContainer = document.getElementById('app');
        this.gameContainer = document.getElementById('game');
        this.videoContainer = document.getElementById('video');
    }

    constructor() {
        this.loadContainers();

        this.stream = (<any>window).mainStreamPlayer;
        this.gameWrapper = (<any>window).mainGameWrapper;

        if (this.gameWrapper == undefined) this.gameWrapper = null;
        if (this.stream == undefined) this.stream = null;

        if (this.gameWrapper != null) this.game = this.gameWrapper.game;
        if (this.gameWrapper != null) this.gameWrapper.onGameUpdate = () => {
            if (this.game != null && this.game.isRunning) {
                if (this.onEnterFrame != null) this.onEnterFrame();
            }
        };

        if (this.game != null) this.game.paused = true;
        if (this.stream != null) this.stream.close();

        console.log(this.game);
    }

    getSpriteGroup(groupName: string): any {
        if (this.gameWrapper == null || this.game == null) return null;
        for (let group of this.gameWrapper.groups) {
            if (group.name == groupName) return group;
        }
        return null;
    }

    getSprite(groupName: string, spriteName: string): any {
        let group = this.getSpriteGroup(groupName);
        if (group != null) return group.getByName(spriteName);
        return null;
    }

    getSpriteImage(groupName: string, spriteName: string): string {
        let spr = this.getSprite(groupName, spriteName);
        if (spr != undefined && spr != null) return spr.key;
        return "";
    }

    updateSpriteImage(groupName: string, spriteName: string, imageName: string) {
        let spr = this.getSprite(groupName, spriteName);
        if (spr == undefined || spr == null) return;
        if (imageName != undefined && imageName != null && imageName.length > 0) {
            if (imageName != spr.key) spr.loadTexture(imageName);
        }
    }

    updateSpritePosition(groupName: string, spriteName: string, x: number, y: number) {
        let spr = this.getSprite(groupName, spriteName);
        if (spr == undefined || spr == null) return;
        spr.x = x;
        spr.y = y;
    }

}

