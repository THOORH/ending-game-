import { Entity } from "../core/Entity";
import { InputManager } from "../input/InputManager";
import { Vector2 } from "../math/Vector2";
import { Animator } from "../systems/AnimationSystem";

/**
 * Entidad del Jugador.
 */
export class CustomPlayer extends Entity {
  public speed: number = 200;
  public health: number = 100;
  public maxHealth: number = 100;
  
  public isDashing: boolean = false;
  private dashCooldown: number = 0;
  private readonly dashDuration: number = 0.2; // s
  private readonly dashSpeed: number = 600;
  private dashTimeLeft: number = 0;

  private animator: Animator = new Animator();

  constructor(id: string) {
    super(id);
    this.transform.position = new Vector2(100, 100);
    this.width = 32;
    this.height = 32;

    this.setupAnimations();
  }

  private setupAnimations() {
    this.animator.add({
      name: "idle",
      loop: true,
      frames: [{ index: 0, duration: 0.1 }, { index: 1, duration: 0.1 }, { index: 2, duration: 0.1 }, { index: 3, duration: 0.1 }, { index: 4, duration: 0.1 }, { index: 5, duration: 0.1 }]
    });

    this.animator.add({
      name: "run",
      loop: true,
      frames: [{ index: 16, duration: 0.08 }, { index: 17, duration: 0.08 }, { index: 18, duration: 0.08 }, { index: 19, duration: 0.08 }, { index: 20, duration: 0.08 }, { index: 21, duration: 0.08 }]
    });

    this.animator.play("idle");
  }

  update(dt: number) {
    // Aplicar velocidad (Knockback)
    this.transform.position.x += this.transform.velocity.x * dt;
    this.transform.position.y += this.transform.velocity.y * dt;

    // Actualizar animación
    this.animator.update(dt);

    // Manejar Cooldowns
    if (this.dashCooldown > 0) this.dashCooldown -= dt;

    // Lógica de Dash
    if (this.isDashing) {
      this.dashTimeLeft -= dt;
      if (this.dashTimeLeft <= 0) {
        this.isDashing = false;
      }
      return;
    }

    // Movimiento normal
    const moveX = InputManager.getHorizontal();
    const moveY = InputManager.getVertical();

    if (moveX !== 0 || moveY !== 0) {
      const moveDir = new Vector2(moveX, moveY).normalize();
      this.transform.position.x += moveDir.x * this.speed * dt;
      this.transform.position.y += moveDir.y * this.speed * dt;
      this.animator.play("run");
    } else {
      this.animator.play("idle");
    }

    // Activar Dash
    if (InputManager.isPressed(" ") && this.dashCooldown <= 0) {
      this.startDash(moveX, moveY);
    }
  }

  private startDash(x: number, y: number) {
    this.isDashing = true;
    this.dashTimeLeft = this.dashDuration;
    this.dashCooldown = 1.0; // 1s cooldown

    const dashDir = (x !== 0 || y !== 0) 
      ? new Vector2(x, y).normalize() 
      : new Vector2(1, 0); // Dash hacia adelante por defecto

    this.transform.position.x += dashDir.x * this.dashSpeed * this.dashDuration;
    this.transform.position.y += dashDir.y * this.dashSpeed * this.dashDuration;
  }

  render(ctx: CanvasRenderingContext2D) {
    // Dibujar placeholder (Rectángulo azul)
    ctx.fillStyle = this.isDashing ? "#00ffff" : "#0000ff";
    ctx.fillRect(
      this.transform.position.x,
      this.transform.position.y,
      this.width,
      this.height
    );

    // Placeholder de texto para mostrar el frame actual (desacoplado)
    ctx.fillStyle = "white";
    ctx.font = "10px Arial";
    ctx.fillText(`F: ${this.animator.getCurrentFrameIndex()}`, this.transform.position.x + 5, this.transform.position.y + 20);

    // Barra de vida lógica sobre la cabeza
    const healthBarWidth = this.width;
    ctx.fillStyle = "red";
    ctx.fillRect(this.transform.position.x, this.transform.position.y - 10, healthBarWidth, 5);
    ctx.fillStyle = "green";
    ctx.fillRect(this.transform.position.x, this.transform.position.y - 10, healthBarWidth * (this.health / this.maxHealth), 5);
  }

  takeDamage(amount: number) {
    this.health -= amount;
    if (this.health < 0) this.health = 0;
  }

  public getDashCooldownProgress(): number {
    return Math.max(0, this.dashCooldown / 1.0); // 1.0 es el cooldown total
  }
}
