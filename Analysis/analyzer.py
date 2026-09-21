"""
Binocular Rivalry VR Experiment - Data Analysis Module
"""

import os
import sys
import json
from pathlib import Path
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
from matplotlib.figure import Figure
import seaborn as sns
from typing import List, Dict, Any, Tuple, Optional

# Canonical direction vectors matching KeyboardInput.cs & FlowController.cs
CANONICAL_DIRECTIONS = {
    "up": np.array([0.0, 1.0]),
    "down": np.array([0.0, -1.0]),
    "right": np.array([1.0, 0.0]),
    "left": np.array([-1.0, 0.0]),
    "up-left": np.array([-1.0, 1.0]) / np.sqrt(2.0),
    "down-right": np.array([1.0, -1.0]) / np.sqrt(2.0),
    "up-right": np.array([1.0, 1.0]) / np.sqrt(2.0),
    "down-left": np.array([-1.0, -1.0]) / np.sqrt(2.0),
}

def get_candidate_data_dirs() -> List[Path]:
    """Returns an ordered list of candidate directories where experiment JSONs may reside."""
    candidates = []
    
    # 1. Project-level Data/ folder (on same level as Analysis)
    try:
        module_dir = Path(__file__).resolve().parent
        candidates.append(module_dir.parent / "Data")
        candidates.append(module_dir.parent / "data")
    except Exception:
        pass
        
    candidates.append(Path("../Data"))
    candidates.append(Path("./Data"))
    candidates.append(Path("Data"))
    candidates.append(Path("../data"))
    candidates.append(Path("./data"))
    candidates.append(Path("data"))
    
    # 2. Standard Unity persistentDataPath on Windows
    try:
        home_path = Path.home()
        candidates.append(home_path / "AppData" / "LocalLow" / "DefaultCompany" / "vr-perceptual-experiment")
    except Exception:
        pass

    user_profile = os.environ.get("USERPROFILE")
    if user_profile:
        candidates.append(Path(user_profile) / "AppData" / "LocalLow" / "DefaultCompany" / "vr-perceptual-experiment")
        
    local_app_data = os.environ.get("LOCALAPPDATA")
    if local_app_data:
        candidates.append(Path(local_app_data).parent / "LocalLow" / "DefaultCompany" / "vr-perceptual-experiment")
        
    candidates.append(Path(r"C:\Users\Asus\AppData\LocalLow\DefaultCompany\vr-perceptual-experiment"))
    candidates.append(Path("."))
    candidates.append(Path(".."))
    
    return candidates

def get_data_dir() -> str:
    """Finds the default data directory containing trial JSON files."""
    candidates = get_candidate_data_dirs()
    
    for cand in candidates:
        try:
            if cand.is_dir():
                json_files = [f for f in cand.glob("*.json") if "participant_" in f.name or "trial_" in f.name]
                if json_files:
                    return str(cand.resolve())
        except Exception:
            continue
            
    for cand in candidates:
        try:
            if cand.is_dir() and cand.name.lower() == "data":
                return str(cand.resolve())
        except Exception:
            continue
            
    return "."

def load_json_file(filepath: str) -> List[Dict[str, Any]]:
    """Loads a participant JSON file and returns a list of trial dictionaries."""
    with open(filepath, "r", encoding="utf-8") as f:
        data = json.load(f)
    if isinstance(data, dict):
        return [data]
    return data

def load_all_trials(data_dir: Optional[str] = None) -> List[Dict[str, Any]]:
    """Loads all participant JSON files from data_dir."""
    if data_dir is None:
        data_dir = get_data_dir()
        
    path_obj = Path(data_dir)
    if not path_obj.is_dir():
        print(f"Warning: '{data_dir}' is not a valid directory.")
        return []

    json_files = sorted(list(path_obj.glob("*.json")))
    all_trials = []
    
    for fpath in json_files:
        fname = fpath.name
        part_id = None
        task_type_from_name = None
        if "participant_" in fname:
            parts = fname.replace(".json", "").split("_")
            try:
                part_id = int(parts[1])
                task_type_from_name = int(parts[3])
            except (IndexError, ValueError):
                pass
                
        try:
            trials = load_json_file(str(fpath))
            loaded_count = 0
            for trial in trials:
                if "spec" not in trial and "logEntries" not in trial:
                    continue
                trial["_source_file"] = fname
                if "participantID" not in trial or trial["participantID"] == 0:
                    trial["participantID"] = part_id if part_id is not None else 0
                if "taskType" not in trial or trial["taskType"] is None:
                    trial["taskType"] = task_type_from_name if task_type_from_name is not None else 1
                all_trials.append(trial)
                loaded_count += 1
            print(f"  - Loaded {loaded_count} trial(s) from {fname}")
        except Exception as e:
            print(f"Warning: Could not parse {fname}: {e}")
            
    return all_trials

def classify_percept(reported_state: str, spec: Dict[str, Any]) -> Tuple[str, str]:
    """Matches reported state to left, right, piecemeal, none."""
    if reported_state == "none":
        return "none", "exact"
    if reported_state == "piecemeal":
        return "piecemeal", "exact"
        
    if reported_state not in CANONICAL_DIRECTIONS:
        return "unknown", "invalid_state"
        
    rep_vec = CANONICAL_DIRECTIONS[reported_state]
    
    dir_l = spec.get("directionL", {})
    dir_r = spec.get("directionR", {})
    
    vec_l = np.array([dir_l.get("x", 0.0), dir_l.get("y", 0.0)])
    vec_r = np.array([dir_r.get("x", 0.0), dir_r.get("y", 0.0)])
    
    norm_l = np.linalg.norm(vec_l)
    norm_r = np.linalg.norm(vec_r)
    
    if norm_l > 0:
        vec_l = vec_l / norm_l
    if norm_r > 0:
        vec_r = vec_r / norm_r

    # Calculate cosine similarity (dot product) between vectors    
    dot_l = np.dot(rep_vec, vec_l)
    dot_r = np.dot(rep_vec, vec_r)
    
    if dot_l > 0.8 and dot_r <= 0.8:
        return "left", "exact"
    if dot_r > 0.8 and dot_l <= 0.8:
        return "right", "exact"
    if dot_l > 0.8 and dot_r > 0.8:
        return ("left" if dot_l >= dot_r else "right"), "control_both"
        
    if dot_l < -0.8 and dot_r >= -0.8:
        return "left", "axis"
    if dot_r < -0.8 and dot_l >= -0.8:
        return "right", "axis"
        
    return "unknown", "ambiguous"

def extract_trial_episodes(trial: Dict[str, Any], default_duration_ms: Optional[float] = None) -> List[Dict[str, Any]]:
    """Extracts contiguous percept episodes with start_ms, end_ms, duration_ms."""
    log_entries = trial.get("logEntries", [])
    task_type = trial.get("taskType", 1)
    spec = trial.get("spec", {})
    
    if default_duration_ms is not None:
        total_duration = default_duration_ms
    else:
        # To differentiate between development and full-length
        max_time = max([e.get("time", 0.0) for e in log_entries], default=10000.0)
        total_duration = 60000.0 if max_time > 15000.0 else 10000.0
        
    episodes = []
    
    if task_type in (1, 2):
        current_state = "none"
        start_time = 0.0
        
        for entry in log_entries:
            t = entry.get("time", 0.0)
            st = entry.get("state", "none")
            
            if st != current_state:
                if current_state != "none" and t > start_time:
                    dom_cat, match_q = classify_percept(current_state, spec)
                    episodes.append({
                        "start_ms": start_time,
                        "end_ms": t,
                        "duration_ms": t - start_time,
                        "state": current_state,
                        "dominance": dom_cat,
                        "match_quality": match_q
                    })
                current_state = st
                start_time = t
                
        if current_state != "none" and total_duration > start_time:
            dom_cat, match_q = classify_percept(current_state, spec)
            episodes.append({
                "start_ms": start_time,
                "end_ms": total_duration,
                "duration_ms": total_duration - start_time,
                "state": current_state,
                "dominance": dom_cat,
                "match_quality": match_q
            })
            
    elif task_type in (3, 4):
        for i, entry in enumerate(log_entries):
            start_time = entry.get("time", 0.0)
            st = entry.get("state", "none")
            if st == "none":
                continue
                
            t_end = log_entries[i + 1].get("time", total_duration) if (i + 1 < len(log_entries)) else total_duration
            dur = max(0.0, t_end - start_time)
            
            dom_cat, match_q = classify_percept(st, spec)
            episodes.append({
                "start_ms": start_time,
                "end_ms": t_end,
                "duration_ms": dur,
                "state": st,
                "dominance": dom_cat,
                "match_quality": match_q
            })
            
    return episodes

def compute_trial_metrics(trial: Dict[str, Any], default_duration_ms: Optional[float] = None) -> Dict[str, Any]:
    """
    Computes summary metrics for a trial.
    """
    spec = trial.get("spec", {})
    task_type = trial.get("taskType", 1)
    part_id = trial.get("participantID", 0)
    trial_num = trial.get("trialNumber", 1)
    
    contrast_l = float(spec.get("contrastL", 0.5))
    contrast_r = float(spec.get("contrastR", 0.5))
    is_control = spec.get("isControl", False)
    
    # Difference and ratio in stimulus strength
    contrast_diff = round(abs(contrast_l - contrast_r), 3)
    contrast_max = max(contrast_l, contrast_r)
    contrast_min = min(contrast_l, contrast_r)
    contrast_ratio = round((contrast_max / contrast_min), 3) if contrast_min > 0 else 1.0

    # Unilateral vs Bilateral variable eye detection
    if task_type == 4:
        var_eye = "both"
        contrast_var = contrast_l
        contrast_fix = contrast_l  # We don't really have a fixed eye in this case, but we don't change the variable name for the sake of simplicity
    else:
        if contrast_l != 0.5 and contrast_r == 0.5:
            var_eye = "left"
            contrast_var = contrast_l
            contrast_fix = 0.5
        elif contrast_r != 0.5 and contrast_l == 0.5:
            var_eye = "right"
            contrast_var = contrast_r
            contrast_fix = 0.5
        else:
            var_eye = "equal"
            contrast_var = 0.5
            contrast_fix = 0.5
            
    episodes = extract_trial_episodes(trial, default_duration_ms=default_duration_ms)
    
    log_entries = trial.get("logEntries", [])
    if default_duration_ms is not None:
        total_trial_ms = default_duration_ms
    else:
        max_time = max([e.get("time", 0.0) for e in log_entries], default=10000.0)
        total_trial_ms = 60000.0 if max_time > 15000.0 else 10000.0

    durations = {"left": [], "right": [], "piecemeal": [], "unknown": []}
    for ep in episodes:
        dom = ep["dominance"]
        if dom in durations:
            durations[dom].append(ep["duration_ms"])
            
    tot_left = sum(durations["left"])
    tot_right = sum(durations["right"])
    tot_piece = sum(durations["piecemeal"])
    tot_active = tot_left + tot_right + tot_piece
    tot_none = max(0.0, total_trial_ms - tot_active)
    
    # Predominance / Dominance % (Proposition I)
    if tot_active > 0:
        pct_left = (tot_left / tot_active) * 100.0
        pct_right = (tot_right / tot_active) * 100.0
        pct_piece = (tot_piece / tot_active) * 100.0
        pct_total_rivalry = ((tot_left + tot_right) / tot_active) * 100.0
    else:
        pct_left = np.nan
        pct_right = np.nan
        pct_piece = np.nan
        pct_total_rivalry = np.nan
    
    mean_dur_left = np.mean(durations["left"]) if len(durations["left"]) > 0 else np.nan
    mean_dur_right = np.mean(durations["right"]) if len(durations["right"]) > 0 else np.nan
    mean_dur_piece = np.mean(durations["piecemeal"]) if len(durations["piecemeal"]) > 0 else np.nan
    
    # Stronger vs. Weaker Stimulus Metrics (Proposition II)
    if contrast_l > contrast_r:
        stronger_eye = "left"
        weaker_eye = "right"
        mean_dur_stronger = mean_dur_left
        mean_dur_weaker = mean_dur_right
        pct_stronger = pct_left
        pct_weaker = pct_right
    elif contrast_r > contrast_l:
        stronger_eye = "right"
        weaker_eye = "left"
        mean_dur_stronger = mean_dur_right
        mean_dur_weaker = mean_dur_left
        pct_stronger = pct_right
        pct_weaker = pct_left
    else:
        stronger_eye = "equal"
        weaker_eye = "equal"
        mean_dur_stronger = (mean_dur_left + mean_dur_right) / 2.0
        mean_dur_weaker = mean_dur_stronger
        pct_stronger = (pct_left + pct_right) / 2.0
        pct_weaker = pct_stronger
        
    # Variable vs Fixed metrics
    if var_eye == "left":
        mean_dur_var = mean_dur_left
        mean_dur_fix = mean_dur_right
        pct_var = pct_left
        pct_fix = pct_right
    elif var_eye == "right":
        mean_dur_var = mean_dur_right
        mean_dur_fix = mean_dur_left
        pct_var = pct_right
        pct_fix = pct_left
    else:
        mean_dur_var = (mean_dur_left + mean_dur_right) / 2.0
        mean_dur_fix = mean_dur_var
        pct_var = pct_left
        pct_fix = pct_right
        
    # Alternation rate (Propositions III & IV)
    switch_count = len(episodes) - 1
    trial_minutes = total_trial_ms / 60000.0
    switch_rate_per_min = switch_count / trial_minutes if trial_minutes > 0 else 0.0
    
    return {
        "participantID": part_id,
        "taskType": task_type,
        "trialNumber": trial_num,
        "isControl": is_control,
        "contrastL": contrast_l,
        "contrastR": contrast_r,
        "variableEye": var_eye,
        "contrastVariable": contrast_var,
        "contrastFixed": contrast_fix,
        "contrastDifference": contrast_diff,
        "contrastRatio": contrast_ratio,
        "strongerEye": stronger_eye,
        "weakerEye": weaker_eye,
        "totalTrialMs": total_trial_ms,
        "totalLeftMs": tot_left,
        "totalRightMs": tot_right,
        "totalPiecemealMs": tot_piece,
        "totalNoneMs": tot_none,
        "dominancePctLeft": pct_left,
        "dominancePctRight": pct_right,
        "dominancePctPiecemeal": pct_piece,
        "dominancePctTotal": pct_total_rivalry,
        "predominanceVariable": pct_var,
        "predominanceFixed": pct_fix,
        "predominanceStronger": pct_stronger,
        "predominanceWeaker": pct_weaker,
        "meanDurationLeftMs": mean_dur_left,
        "meanDurationRightMs": mean_dur_right,
        "meanDurationPiecemealMs": mean_dur_piece,
        "meanDurationVariableMs": mean_dur_var,
        "meanDurationFixedMs": mean_dur_fix,
        "meanDurationStrongerMs": mean_dur_stronger,
        "meanDurationWeakerMs": mean_dur_weaker,
        "switchCount": switch_count,
        "switchRatePerMin": switch_rate_per_min,
        "_episodes": episodes
    }

def plot_all_trials_timeline(trials: List[Dict[str, Any]], title: str = "All Trials – Percept Timeline", row_height: float = 0.55, fig_width: float = 14.0,) -> Figure:
    """
    Renders every trial as a horizontal bar row, grouped and ordered by taskType.

    Layout
    ------
    - One row per trial, sorted by (taskType, trialNumber).
    - A thick horizontal separator line + bold task label is drawn between tasks.
    - Colour key: blue = left-eye dominant, red = right-eye dominant,
      yellow = piecemeal, light-green = none, grey = unknown.

    Parameters
    ----------
    trials      : raw trial dicts as returned by load_all_trials().
    title       : overall figure title.
    row_height  : height (inches) allocated to each trial row.
    fig_width   : total figure width in inches.

    Returns
    -------
    matplotlib Figure object (caller is responsible for saving / showing it).
    """
    COLOR_MAP = {
        "left":      "#3498db",  # blue
        "right":     "#e74c3c",  # red
        "piecemeal": "#f1c40f",  # yellow
        "none":      "#d5f5d3",  # very light green
        "unknown":   "#999999",  # grey
    }
    TASK_COLORS = {1: "#1a6ebd", 2: "#27ae60", 3: "#8e44ad", 4: "#d35400"}
    TASK_LABELS = {
        1: "Task 1 – Holding / Unilateral (Prop. I: Predominance)",
        2: "Task 2 – Holding / Unilateral (Prop. II: Duration vs. Difference)",
        3: "Task 3 – Tapping / Unilateral (Prop. III: Alternation Rate vs. Difference)",
        4: "Task 4 – Tapping / Bilateral (Prop. IV: Alternation Rate vs. Bilateral Contrast)",
    }

    # 1. Sort trials by (taskType, trialNumber)
    sorted_trials = sorted(
        trials,
        key=lambda t: (t.get("taskType", 1), t.get("trialNumber", 0)),
    )

    # 2. Build row metadata
    # Each element: (trial_dict, episodes, label_str)
    rows = []
    for t in sorted_trials:
        eps = extract_trial_episodes(t)
        task = t.get("taskType", 1)
        trial_num = t.get("trialNumber", "?")
        spec = t.get("spec", {})
        cl = spec.get("contrastL", "?")
        cr = spec.get("contrastR", "?")
        is_ctrl = spec.get("isControl", False)
        ctrl_tag = " [ctrl]" if is_ctrl else ""
        label = f"T{trial_num}{ctrl_tag}  CL={cl:.2f} CR={cr:.2f}"
        rows.append((t, eps, label, task))

    n_rows = len(rows)
    if n_rows == 0:
        fig, ax = plt.subplots()
        ax.text(0.5, 0.5, "No trials loaded.", ha="center", va="center")
        return fig

    # 3. Create figure
    # Extra vertical space: 1.4 in per task group header + padding
    n_tasks = len({r[3] for r in rows})
    fig_height = max(4.0, n_rows * row_height + n_tasks * 1.4 + 1.5)
    fig, ax = plt.subplots(figsize=(fig_width, fig_height))
    ax.set_xlim(0, 1)  # normalised to [0, 1] – rescaled per trial
    ax.axis("off")     # we draw everything manually

    # 4. Lay out rows top-to-bottom 
    TOP_MARGIN = 0.97        # fraction of figure height
    BOTTOM_MARGIN = 0.03
    usable = TOP_MARGIN - BOTTOM_MARGIN

    # Vertical unit in figure-fraction space
    TASK_SEP_H  = 1.4 / fig_height   # height consumed by each task header
    ROW_H       = row_height / fig_height

    total_units = n_rows * ROW_H + n_tasks * TASK_SEP_H
    # Normalise so everything fits in [BOTTOM_MARGIN, TOP_MARGIN]
    scale = usable / total_units if total_units > 0 else 1.0
    row_h_f   = ROW_H    * scale
    task_sep_f = TASK_SEP_H * scale

    cursor = TOP_MARGIN       # y position in figure-fraction, going DOWN
    prev_task = None

    for (trial_dict, eps, label, task) in rows:
        # Task header separator 
        if task != prev_task:
            # Horizontal divider line
            line_y = cursor - task_sep_f * 0.15
            ax.plot(
                [0.01, 0.99], [line_y, line_y],
                color=TASK_COLORS.get(task, "#333333"),
                linewidth=2.5,
                transform=ax.transAxes,
                zorder=5,
            )
            # Task label text
            label_y = cursor - task_sep_f * 0.6
            ax.text(
                0.5, label_y,
                TASK_LABELS.get(task, f"Task {task}"),
                transform=ax.transAxes,
                ha="center", va="center",
                fontsize=10, fontweight="bold",
                color=TASK_COLORS.get(task, "#333333"),
            )
            cursor -= task_sep_f
            prev_task = task

        # Determine trial time window 
        log_entries = trial_dict.get("logEntries", [])
        max_t = max((e.get("time", 0.0) for e in log_entries), default=10000.0)
        total_ms = 60000.0 if max_t > 15000.0 else 10000.0
        if eps:
            total_ms = max(total_ms, max(e["end_ms"] for e in eps))

        # Row bounding box (in axes-fraction coords) 
        row_top    = cursor
        row_bottom = cursor - row_h_f
        row_mid    = (row_top + row_bottom) / 2.0

        bar_h = row_h_f * 0.70   # bar height within the row slot

        # Draw "none" background for the full trial span 
        ax.barh(
            row_mid, 1.0, left=0.0,
            height=bar_h,
            color=COLOR_MAP["none"],
            edgecolor="none",
            transform=ax.transAxes,
            zorder=2,
        )

        # Draw each episode
        for ep in eps:
            x_start = ep["start_ms"] / total_ms
            x_width = ep["duration_ms"] / total_ms
            dom = ep["dominance"]
            if dom == "none":
                continue  # already drawn as background
            ax.barh(
                row_mid, x_width, left=x_start,
                height=bar_h,
                color=COLOR_MAP.get(dom, COLOR_MAP["unknown"]),
                edgecolor="black", linewidth=0.3,
                transform=ax.transAxes,
                zorder=3,
            )

        # Row label on the left
        ax.text(
            0.002, row_mid,
            label,
            transform=ax.transAxes,
            ha="left", va="center",
            fontsize=6.5,
            color="#222222",
            zorder=6,
        )

        # Thin border around the whole row
        import matplotlib.patches as mpatches
        rect = mpatches.FancyBboxPatch(
            (0.0, row_bottom), 1.0, row_h_f,
            boxstyle="square,pad=0",
            linewidth=0.4,
            edgecolor="#aaaaaa",
            facecolor="none",
            transform=ax.transAxes,
            zorder=4,
        )
        ax.add_patch(rect)

        cursor -= row_h_f

    # 5. Legend & title
    import matplotlib.patches as mpatches
    legend_items = [
        mpatches.Patch(color=COLOR_MAP["left"],      label="Left Eye Dominant"),
        mpatches.Patch(color=COLOR_MAP["right"],     label="Right Eye Dominant"),
        mpatches.Patch(color=COLOR_MAP["piecemeal"], label="Piecemeal"),
        mpatches.Patch(color=COLOR_MAP["none"],      label="None (motor gap)"),
        mpatches.Patch(color=COLOR_MAP["unknown"],   label="Unknown"),
    ]
    ax.legend(
        handles=legend_items,
        loc="lower center",
        bbox_to_anchor=(0.5, 0.0),
        ncol=5,
        fontsize=8,
        framealpha=0.9,
        edgecolor="#cccccc",
    )
    fig.suptitle(title, fontsize=13, fontweight="bold", y=1.0)
    fig.tight_layout(rect=(0.0, 0.04, 1.0, 0.99))
    return fig

def build_summary_dataframe(trials: List[Dict[str, Any]]) -> pd.DataFrame:
    """Builds a comprehensive pandas DataFrame for all trials."""
    records = []
    for t in trials:
        metrics = compute_trial_metrics(t)
        rec = {k: v for k, v in metrics.items() if k != "_episodes"}
        records.append(rec)
    df = pd.DataFrame(records)
    return df
