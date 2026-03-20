import { Entity } from "../core/Entity";
import { Vector2 } from "../math/Vector2";
import { CustomPlayer } from "./CustomPlayer";
import { CombatSystem } from "../systems/CombatSystem";

export enum EnemyState {
  IDLE,
  PATROL,
  CHASE,
  ATTACK,
}

/**
 * Clase base para enemigos con IA básica.
 */
export class BasicEnemy extends Entity {
  public state: EnemyState = EnemyState.PATROL;
  public speed: number = 100;
  public chaseRange: number = 200;
  public attackRange: number = 40;
  public health: number = 50;
  public maxHealth: number = 50;

  private patrolPoints: Vector2[] = [];
  private currentPatrolIndex: number = 0;
  private target: CustomPlayer | null = null;

  constructor(id: string, patrolPoints: Vector2[]) {
    super(id);
    this.patrolPoints = patrolPoints;
    this.transform.position = patrolPoints[0].copy();
    this.width = 32;
    this.height = 32;
  }

  setTarget(player: CustomPlayer) {
    this.target = player;
  }

  update(dt: number) {
    if (!this.target) return;

    // Aplicar velocidad (Knockback)
    this.transform.position.x += this.transform.velocity.x * dt;
    this.transform.position.y += this.transform.velocity.y * dt;
    CombatSystem.applyFriction(this, dt);

    const distToPlayer = Vector2.distance(this.transform.position, this.target.transform.position);

    // Máquina de estados finita (FSM)
    switch (this.state) {
      case EnemyState.PATROL:
        this.patrolUpdate(dt);
        if (distToPlayer < this.chaseRange) {
          this.state = EnemyState.CHASE;
        }
        break;

      case EnemyState.CHASE:
        this.chaseUpdate(dt);
        if (distToPlayer < this.attackRange) {
          this.state = EnemyState.ATTACK;
        } else if (distToPlayer > this.chaseRange * 1.5) {
          this.state = EnemyState.PATROL;
        }
        break;

      case EnemyState.ATTACK:
        this.attackUpdate(dt);
        if (distToPlayer > this.attackRange * 1.2) {
          this.state = EnemyState.CHASE;
        }
        break;

      default:
        break;
    }
  }

  private patrolUpdate(dt: number) {
    const targetPatrol = this.patrolPoints[this.currentPatrolIndex];
    const dist = Vector2.distance(this.transform.position, targetPatrol);

    if (dist < 5) {
      this.currentPatrolIndex = (this.currentPatrolIndex + 1) % this.patrolPoints.length;
    }

    const moveDir = new Vector2(targetPatrol.x - this.transform.position.x, targetPatrol.y - this.transform.position.y).normalize();
    this.transform.position.x += moveDir.x * this.speed * 0.5 * dt;
    this.transform.position.y += moveDir.y * this.speed * 0.5 * dt;
  }

  private chaseUpdate(dt: number) {
    if (!this.target) return;
    const moveDir = new Vector2(this.target.transform.position.x - this.transform.position.x, this.target.transform.position.y - this.transform.position.y).normalize();
    this.transform.position.x += moveDir.x * this.speed * dt;
    this.transform.position.y += moveDir.y * this.speed * dt;
  }

  private attackUpdate(dt: number) {
    if (this.target) {
      // Daño por contacto básico
      CombatSystem.dealDamage(this, this.target, 5 * dt, 100);
    }
  }

  render(ctx: CanvasRenderingContext2D) {
    // Color según estado para debugging visual
    ctx.fillStyle = this.state === EnemyState.CHASE ? "orange" : (this.state === EnemyState.ATTACK ? "red" : "gray");
    ctx.fillRect(this.transform.position.x, this.transform.position.y, this.width, this.height);
    
    // Vida del enemigo
    ctx.fillStyle = "black";
    ctx.fillRect(this.transform.position.x, this.transform.position.y - 10, this.width, 4);
    ctx.fillStyle = "red";
    ctx.fillRect(this.transform.position.x, this.transform.position.y - 10, this.width * (this.health / this.maxHealth), 4);
  }
}
