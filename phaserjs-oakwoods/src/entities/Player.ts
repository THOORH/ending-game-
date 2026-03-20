import Phaser from "phaser";
import { Dash } from "../systems/Dash";

export class Player extends Phaser.Physics.Arcade.Sprite {
  public health: number = 100;
  public maxHealth: number = 100;
  
  public isDashing: boolean = false;
  public dashCooldown: number = 0;
  private dashSystem: Dash;

  private isAttacking: boolean = false;
  private isDead: boolean = false;
  private isInvulnerable: boolean = false;
  private invulnerabilityDuration: number = 1000; // 1s de i-frames

  constructor(scene: Phaser.Scene, x: number, y: number) {
    super(scene, x, y, "oakwoods-char-blue", 0);
    
    this.dashSystem = new Dash();
    
    scene.add.existing(this);
    scene.physics.add.existing(this);

    this.setBounce(0);
    this.body?.setSize(20, 38);
    this.body?.setOffset(18, 16);
    this.setCollideWorldBounds(true);
    this.body?.setBoundsRectangle(new Phaser.Geom.Rectangle(0, 0, 999999, 180));

    // Reset attack state when animation finishes
    this.on("animationcomplete", (anim: any) => {
      if (anim.key === "char-blue-attack") {
        this.isAttacking = false;
      }
    });
  }

  update(cursors: Phaser.Types.Input.Keyboard.CursorKeys, dashKey: Phaser.Input.Keyboard.Key, attackKey: Phaser.Input.Keyboard.Key): void {
    if (this.isDead) return;

    const body = this.body as Phaser.Physics.Arcade.Body;
    if (!body) return;

    if (this.isDashing) return;

    // Movement logic
    let dx = 0;
    if (cursors.left.isDown) {
      dx = -1;
      this.setFlipX(true);
      if (!this.isAttacking) this.anims.play("char-blue-run", true);
    } else if (cursors.right.isDown) {
      dx = 1;
      this.setFlipX(false);
      if (!this.isAttacking) this.anims.play("char-blue-run", true);
    } else {
      if (!this.isAttacking) this.anims.play("char-blue-idle", true);
    }

    const speed = 160;
    this.setVelocityX(dx * speed);

    // Jump logic
    if (cursors.up.isDown && body.blocked.down) {
      this.setVelocityY(-330);
    }

    // Animation states for jumping/falling
    if (!body.blocked.down && !this.isAttacking) {
      if (body.velocity.y < 0) {
        this.anims.play("char-blue-jump", true);
      } else {
        this.anims.play("char-blue-fall", true);
      }
    }

    // Attack logic
    if (Phaser.Input.Keyboard.JustDown(attackKey) && !this.isAttacking && body.blocked.down) {
      this.isAttacking = true;
      this.setVelocityX(0);
      this.anims.play("char-blue-attack", true);
    }

    // Dash logic
    if (Phaser.Input.Keyboard.JustDown(dashKey)) {
      this.dashSystem.tryDash(this, dx);
    }

    // Cooldown update
    this.dashSystem.update(this, 16.67);
  }

  public takeDamage(amount: number): void {
    if (this.isDead || this.isInvulnerable || this.isDashing) return;

    this.health -= amount;
    
    if (this.health <= 0) {
      this.health = 0;
      this.die();
      return;
    }
    
    // Iniciar invulnerabilidad (i-frames)
    this.isInvulnerable = true;
    
    // Feedback visual (parpadeo)
    this.scene.tweens.add({
      targets: this,
      alpha: 0.2,
      duration: 100,
      ease: "Linear",
      repeat: 5,
      yoyo: true,
      onComplete: () => {
        this.setAlpha(1);
        this.isInvulnerable = false;
      }
    });

    // Retroalimentación de color (opcional)
    this.setTint(0xff0000);
    this.scene.time.delayedCall(200, () => {
      this.clearTint();
    });
  }

  private die(): void {
    if (this.isDead) return;
    this.isDead = true;
    
    this.setVelocity(0, 0);
    this.setTint(0x555555); // Gris muerto
    this.anims.stop();
    
    // Emitir evento de muerte para que la escena lo maneje
    this.emit("player_died");
  }

  public getIsDead(): boolean {
    return this.isDead;
  }

  public getIsAttacking(): boolean {
    return this.isAttacking;
  }

  public getIsInvulnerable(): boolean {
    return this.isInvulnerable;
  }

  public getDashCooldownProgress(): number {
    return Math.max(0, this.dashCooldown / this.dashSystem.cooldownTime);
  }
}
