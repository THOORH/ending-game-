import { Scene } from "../core/Scene";
import { CustomPlayer } from "../entities/CustomPlayer";
import { BasicEnemy } from "../entities/BasicEnemy";
import { Vector2 } from "../math/Vector2";
import { CollisionSystem } from "../systems/CollisionSystem";
import { CustomHUD } from "../ui/CustomHUD";
import { CombatSystem } from "../systems/CombatSystem";

/**
 * Escena de prueba del motor personalizado.
 */
export class DevScene extends Scene {
  private player!: CustomPlayer;
  private enemies: BasicEnemy[] = [];
  private staticHazards: { x: number; y: number; w: number; h: number }[] = [];
  private hud!: CustomHUD;

  init() {
    this.player = new CustomPlayer("player_1");
    this.hud = new CustomHUD(this.player);

    // Crear un enemigo patrullando un cuadrado
    const patrolPoints = [
      new Vector2(300, 100),
      new Vector2(500, 100),
      new Vector2(500, 300),
      new Vector2(300, 300)
    ];
    const enemy = new BasicEnemy("enemy_1", patrolPoints);
    enemy.setTarget(this.player);
    this.enemies.push(enemy);

    // Obstáculos estáticos
    this.staticHazards.push({ x: 200, y: 200, w: 50, h: 50 });
  }

  update(dt: number) {
    this.player.update(dt);
    CombatSystem.applyFriction(this.player, dt);

    this.enemies.forEach(enemy => {
      enemy.update(dt);

      // Colisión Jugador vs Enemigo
      if (CollisionSystem.checkCollision(this.player, enemy)) {
        // Lógica de daño con knockback usando el nuevo sistema
        if (!this.player.isDashing) {
          CombatSystem.dealDamage(enemy, this.player, 10 * dt, 200);
        }
        
        // Resolución física de colisión
        CollisionSystem.resolveCollision(this.player, enemy);
      }
    });

    // Colisión Jugador vs Obstáculos
    this.staticHazards.forEach(h => {
      const hazardEntity: any = { 
        transform: { position: new Vector2(h.x, h.y) }, 
        width: h.w, 
        height: h.h 
      };
      CollisionSystem.resolveCollision(this.player, hazardEntity);
    });
  }

  render(ctx: CanvasRenderingContext2D) {
    // Fondo claro
    ctx.fillStyle = "#f0f0f0";
    ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    // Dibujar obstáculos
    ctx.fillStyle = "#333";
    this.staticHazards.forEach(h => {
      ctx.fillRect(h.x, h.y, h.w, h.h);
    });

    // Dibujar entidades
    this.player.render(ctx);
    this.enemies.forEach(enemy => enemy.render(ctx));

    // Dibujar HUD
    this.hud.render(ctx);

    // Instrucciones
    ctx.fillStyle = "black";
    ctx.font = "14px Arial";
    ctx.fillText("WASD/Arrows: Mover | Espacio: Dash", 10, 20);
    ctx.fillText("Gris: Patrulla | Naranja: Persecución | Rojo: Ataque", 10, 40);
  }

  cleanup() {
    this.enemies = [];
  }
}
