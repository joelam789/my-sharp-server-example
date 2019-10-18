
	function SimpleGameWrapper (w, h, renderMode, container, onPreload, onCreate) {
		
		this.groups = [];
		this.onGameUpdate = null;
		
		this.update = function() {
			if (this.onGameUpdate != null) {
				if (this.game != null && this.game.isRunning) {
					this.onGameUpdate();
				}
			}
		};
		
		this.game = new Phaser.Game(w, h, renderMode, container, 
						{ preload: onPreload, 
						  create: onCreate, 
						  update: this.update.bind(this)
						},
						true);
		
	}
