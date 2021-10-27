﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OperationBlackwell.Core;

namespace OperationBlackwell.Player {
	public class UnitGridCombat : CoreUnit {

		[SerializeField] private Team team_;
		[SerializeField] private int actionPoints_;
		[SerializeField] private int maxActionPoints_;
		
		private PlayerBase characterBase_;
		private GameObject selectedGameObject_;
		private MovePositionPathfinding movePosition_;
		private State state_;
		private HealthSystem healthSystem_;

		private enum State {
			Normal,
			Moving,
			Attacking
		}

		private void Awake() {
			characterBase_ = GetComponent<PlayerBase>();
			selectedGameObject_ = transform.Find("Selected").gameObject;
			movePosition_ = GetComponent<MovePositionPathfinding>();
			//SetSelectedVisible(false);
			state_ = State.Normal;
			healthSystem_ = new HealthSystem(100);
			healthSystem_.OnHealthChanged += HealthSystem_OnHealthChanged;
		}

		private void Update() {
			switch(state_) {
				case State.Normal:
					break;
				case State.Moving:
					break;
				case State.Attacking:
					break;
				default: 
					break;
			}
		}

		private void OnDestroy() {
			healthSystem_.OnHealthChanged -= HealthSystem_OnHealthChanged;
		}

		private void HealthSystem_OnHealthChanged(object sender, EventArgs e) {
			// healthBar.SetSize(healthSystem.GetHealthNormalized());
		}

		public void SetSelectedVisible(bool visible) {
			selectedGameObject_.SetActive(visible);
		}

		public override bool CanAttackUnit(CoreUnit unitGridCombat) {
			/* 
			 * TODO: Check if unit is in range
			 * TODO: Check if unit is on the same team
			 * The value of 1.5f is a placeholder for the range of the units attack.
			 */
			return Vector3.Distance(GetPosition(), unitGridCombat.GetPosition()) < 1.5f;
		}

		public override void MoveTo(Vector3 targetPosition, Action onReachedPosition) {
			state_ = State.Moving;
			movePosition_.SetMovePosition(targetPosition, () => {
				state_ = State.Normal;
				onReachedPosition();
			});
		}

		public override Vector3 GetPosition() {
			return transform.position;
		}

		public override Team GetTeam() {
			return team_;
		}

		public override bool IsEnemy(CoreUnit unitGridCombat) {
			return unitGridCombat.GetTeam() != team_;
		}

		public override void SetActionPoints(int actionPoints) {
			actionPoints_ = actionPoints;
		}

		public override int GetActionPoints() {
			return actionPoints_;
		}

		public override void ResetActionPoints() {
			actionPoints_ = maxActionPoints_;
		}

		public override void AttackUnit(CoreUnit unitGridCombat, Action onAttackComplete) {
			state_ = State.Attacking;

			ShootUnit(unitGridCombat, () => {
				state_ = State.Normal;
				onAttackComplete(); 
			});
		}

		private void ShootUnit(CoreUnit unitGridCombat, Action onShootComplete) {
			GetComponent<IMoveVelocity>().Disable();

			// The value of 50 is a placeholder for the damage of the units attack.
			unitGridCombat.Damage(this, 50); //UnityEngine.Random.Range(4, 12));

			GetComponent<IMoveVelocity>().Enable();
			onShootComplete();
		}

		public override void Damage(CoreUnit attacker, int damageAmount) {	
			healthSystem_.Damage(damageAmount);
			if(healthSystem_.IsDead()) {
				GridCombatSystem.Instance.OnUnitDeath?.Invoke(this, EventArgs.Empty);
				Destroy(gameObject);
			} else {
				// Knockback
				//transform.position += bloodDir * 5f;
			}
		}

		public override bool IsDead() {
			return healthSystem_.IsDead();
		}
	}
}
