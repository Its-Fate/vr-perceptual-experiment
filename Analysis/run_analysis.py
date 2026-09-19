"""
Run Post-Experiment Analysis from Command Line
----------------------------------------------
Executes the 1-to-1 Task-Proposition analysis pipeline:
  Task 1 -> Proposition I (Predominance)
  Task 2 -> Proposition II (Mean Duration vs Difference)
  Task 3 -> Proposition III (Alternation Rate vs Difference)
  Task 4 -> Proposition IV (Alternation Rate vs Bilateral Contrast)

Usage:
    python run_analysis.py [optional_path_to_data_dir]
"""

import os
import sys
from pathlib import Path
import matplotlib.pyplot as plt
import seaborn as sns

sys.path.insert(0, str(Path(__file__).resolve().parent))
import analyzer

def main():
    data_dir = sys.argv[1] if len(sys.argv) > 1 else analyzer.get_data_dir()
    print(f"[1/5] Ingesting data from: {data_dir}")
    
    trials = analyzer.load_all_trials(data_dir)
    if not trials:
        print("No trial JSON files found.")
        return
        
    print(f"Loaded {len(trials)} trial(s).")
    
    output_dir = os.path.join(os.path.dirname(__file__), "output")
    os.makedirs(output_dir, exist_ok=True)
    
    print("[2/5] Computing metrics...")
    df = analyzer.build_summary_dataframe(trials)
    
    # 1. Export CSV
    csv_path = os.path.join(output_dir, "perceptual_metrics_summary.csv")
    df.to_csv(csv_path, index=False)
    print(f"\n[3/5] Saved summary dataset to: {csv_path}")
    
    print("[4/5] Generating 1-to-1 Task <-> Proposition Figures...")
    sns.set_theme(style="whitegrid", font_scale=1.1)
    
    t1_df = df[df["taskType"] == 1]
    t2_df = df[df["taskType"] == 2]
    t3_df = df[df["taskType"] == 3]
    t4_df = df[df["taskType"] == 4]
    
    # Task 1 -> Proposition I: Predominance
    if not t1_df.empty:
        summary_p1 = t1_df.groupby("contrastVariable")[["predominanceVariable", "predominanceFixed", "dominancePctPiecemeal"]].mean().reset_index()
        plt.figure(figsize=(9, 5))
        plt.plot(summary_p1["contrastVariable"], summary_p1["predominanceVariable"], marker="o", color="#2980b9", lw=2.5, label="Variable Eye Predominance (%)")
        plt.plot(summary_p1["contrastVariable"], summary_p1["predominanceFixed"], marker="s", color="#e67e22", lw=2.5, label="Fixed Eye Predominance (%)")
        plt.plot(summary_p1["contrastVariable"], summary_p1["dominancePctPiecemeal"], marker="^", color="#f1c40f", lw=2, linestyle="--", label="Piecemeal (%)")
        plt.title("Task 1: Proposition I - Predominance vs. Stimulus Strength", fontsize=14, fontweight="bold")
        plt.xlabel("Variable Eye Contrast Level")
        plt.ylabel("Predominance (%)")
        plt.ylim(0, 100)
        plt.legend()
        plt.tight_layout()
        plt.savefig(os.path.join(output_dir, "task1_prop1_predominance.png"), dpi=150)
        plt.close()
        print("  - Generated Task 1 (Prop I) plot.")
    else:
        print("  - Task 1: No data found yet.")
        
    # Task 2 -> Proposition II: Mean Dominance Duration vs. Interocular Difference
    if not t2_df.empty:
        summary_p2 = t2_df.groupby("contrastDifference")[["meanDurationStrongerMs", "meanDurationWeakerMs"]].mean().reset_index()
        plt.figure(figsize=(9, 5))
        plt.plot(summary_p2["contrastDifference"], summary_p2["meanDurationStrongerMs"] / 1000.0, marker="o", lw=2.5, color="#27ae60", label="Stronger Stimulus Mean Duration (s)")
        plt.plot(summary_p2["contrastDifference"], summary_p2["meanDurationWeakerMs"] / 1000.0, marker="s", lw=2.5, color="#c0392b", label="Weaker Stimulus Mean Duration (s)")
        plt.title("Task 2: Proposition II - Dominance Duration vs. Interocular Difference (|CL - CR|)", fontsize=14, fontweight="bold")
        plt.xlabel("Difference in Stimulus Strength (|CL - CR|)")
        plt.ylabel("Mean Duration (seconds)")
        plt.legend()
        plt.tight_layout()
        plt.savefig(os.path.join(output_dir, "task2_prop2_duration_vs_diff.png"), dpi=150)
        plt.close()
        print("  - Generated Task 2 (Prop II) plot.")
    else:
        print("  - Task 2: No data found yet.")
        
    # Task 3 -> Proposition III: Alternation Rate vs. Interocular Difference
    if not t3_df.empty:
        summary_p3 = t3_df.groupby("contrastDifference")["switchRatePerMin"].mean().reset_index()
        plt.figure(figsize=(9, 5))
        plt.plot(summary_p3["contrastDifference"], summary_p3["switchRatePerMin"], marker="o", color="#8e44ad", lw=2.5, label="Alternation Rate")
        plt.title("Task 3: Proposition III - Alternation Rate vs. Interocular Difference (|CL - CR|)", fontsize=14, fontweight="bold")
        plt.xlabel("Difference in Stimulus Strength (|CL - CR|)")
        plt.ylabel("Alternation Rate (switches / min)")
        plt.legend()
        plt.tight_layout()
        plt.savefig(os.path.join(output_dir, "task3_prop3_alternation_vs_diff.png"), dpi=150)
        plt.close()
        print("  - Generated Task 3 (Prop III) plot.")
    else:
        print("  - Task 3: No data found yet.")
        
    # Task 4 -> Proposition IV: Alternation Rate vs. Bilateral Contrast
    if not t4_df.empty:
        summary_p4 = t4_df.groupby("contrastL")["switchRatePerMin"].mean().reset_index()
        plt.figure(figsize=(9, 5))
        plt.plot(summary_p4["contrastL"], summary_p4["switchRatePerMin"], marker="s", color="#d35400", lw=2.5, label="Equal Bilateral Contrast (CL = CR)")
        plt.title("Task 4: Proposition IV - Alternation Rate vs. Bilateral Stimulus Strength", fontsize=14, fontweight="bold")
        plt.xlabel("Bilateral Stimulus Contrast (Both Eyes)")
        plt.ylabel("Alternation Rate (switches / min)")
        plt.legend()
        plt.tight_layout()
        plt.savefig(os.path.join(output_dir, "task4_prop4_bilateral_alternation.png"), dpi=150)
        plt.close()
        print("  - Generated Task 4 (Prop IV) plot.")
    else:
        print("  - Task 4: No data found yet.")
        
    print(f"[5/5] Analysis complete! Output figures and summary CSV written to: {output_dir}")

if __name__ == "__main__":
    main()
